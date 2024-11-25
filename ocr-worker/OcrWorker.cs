using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using ImageMagick;
using Tesseract;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("DocumentManagementSystem.Tests")]

namespace ocr_worker
{
    public class OcrWorker : IDisposable
    {
        private IConnection _connection;
        private IModel _channel;

        // Dependency Injection Constructor
        public OcrWorker(IConnection connection = null, IModel channel = null)
        {
            if (connection == null || channel == null)
            {
                ConnectToRabbitMQ(); // Ensure the connection and channel are initialized
            }
            else
            {
                _connection = connection;
                _channel = channel;
            }
            // Ensure _channel is initialized before calling BasicConsume
            if (_channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");
            }
        }

        // Private Constructor to Initialize and Connect to RabbitMQ
        private OcrWorker()
        {
            ConnectToRabbitMQ();
        }

        // Connects to RabbitMQ with retries
        private void ConnectToRabbitMQ()
        {
            int retries = 5;
            while (retries > 0)
            {
                try
                {
                    var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "user", Password = "password" };
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.QueueDeclare(queue: "document_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    Console.WriteLine("Successfully connected to RabbitMQ and created queue.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to RabbitMQ: {ex.Message}. Retrying in 5 seconds...");
                    Thread.Sleep(5000);
                    retries--;
                }
            }

            if (_connection == null || !_connection.IsOpen)
            {
                throw new Exception("Failed to connect to RabbitMQ after several attempts.");
            }
        }

        // Starts consuming messages from RabbitMQ and processes them
        public void Start()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var parts = message.Split('|');

                if (parts.Length == 2)
                {
                    var id = parts[0];
                    var filePath = parts[1];
                    Console.WriteLine($"[x] Received ID: {id}, FilePath: {filePath}");

                    // Validate that the file exists
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine($"Error: File {filePath} not found.");
                        return;
                    }

                    // Perform OCR on the document
                    var extractedText = PerformOcr(filePath);
                    if (!string.IsNullOrEmpty(extractedText))
                    {
                        // Send OCR result back to RabbitMQ
                        var resultBody = Encoding.UTF8.GetBytes($"{id}|{extractedText}");
                        _channel.BasicPublish(exchange: "", routingKey: "ocr_result_queue", basicProperties: null, body: resultBody);
                        Console.WriteLine($"[x] Sent OCR result for ID: {id}");
                    }
                    else
                    {
                        Console.WriteLine("Error: Extracted text is null or empty.");
                    }
                }
                else
                {
                    Console.WriteLine("Error: Invalid message received, split less than 2 parts.");
                }
            };
            if (_channel == null)
            {
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");
            }
            _channel.BasicConsume(queue: "document_queue", autoAck: true, consumer: consumer);
        }

        // Internal method for OCR processing (so it can be tested)
        internal string PerformOcr(string filePath)
        {
            Console.WriteLine($"Attempting OCR on file: {filePath}");
            var stringBuilder = new StringBuilder();

            try
            {
                using (var images = new MagickImageCollection(filePath)) // MagickImageCollection for multiple pages
                {
                    foreach (var image in images)
                    {
                        Console.WriteLine("Processing page...");

                        // Convert image to PNG
                        var tempPngFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

                        image.Density = new Density(300, 300); // High DPI for better OCR accuracy
                        image.Contrast(); // Improve contrast for OCR
                        image.Sharpen();  // Sharpen image for better text detection
                        image.Write(tempPngFile);
                        Console.WriteLine($"Temporary PNG created at: {tempPngFile}");

                        // Run Tesseract OCR on the image
                        var result = RunTesseract(tempPngFile);
                        stringBuilder.Append(result);
                        Console.WriteLine($"OCR Result: {result}");

                        // Clean up temporary PNG file
                        DeleteTempFile(tempPngFile);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during OCR processing: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                }
            }

            return stringBuilder.ToString();
        }

        // Helper method to run Tesseract OCR process
        private string RunTesseract(string tempPngFile)
        {
            var stringBuilder = new StringBuilder();
            var psi = new ProcessStartInfo
            {
                FileName = "tesseract",
                Arguments = $"{tempPngFile} stdout -l eng",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    string result = process.StandardOutput.ReadToEnd();
                    stringBuilder.Append(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during Tesseract execution: {ex.Message}");
            }

            return stringBuilder.ToString();
        }

        // Helper method to delete temporary files
        private void DeleteTempFile(string tempPngFile)
        {
            try
            {
                if (File.Exists(tempPngFile))
                {
                    File.Delete(tempPngFile);
                    Console.WriteLine("Temporary PNG deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting temporary PNG file: {ex.Message}");
            }
        }

        // Dispose method to clean up RabbitMQ connection and channel
        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
