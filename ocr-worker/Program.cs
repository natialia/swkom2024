using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Tesseract;

class Program
{
    private const string InputQueueName = "document_uploads";
    private const string OutputQueueName = "ocr_results";

    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: InputQueueName, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(queue: OutputQueueName, durable: true, exclusive: false, autoDelete: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"Received: {message}");

            // Perform OCR
            var extractedText = PerformOcr(message);

            // Send the result back
            var resultBytes = Encoding.UTF8.GetBytes($"{ea.RoutingKey}|{extractedText}");
            channel.BasicPublish(exchange: "", routingKey: OutputQueueName, basicProperties: null, body: resultBytes);
            Console.WriteLine($"Sent OCR result for {message}");
        };


        channel.BasicConsume(queue: InputQueueName, autoAck: true, consumer: consumer);

        Console.WriteLine("OCR Service running. Press [enter] to exit.");
        Console.ReadLine();
    }

    private static string PerformOcr(string filePath)
    {
        try
        {
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(filePath);
            using var page = engine.Process(img);
            return page.GetText();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OCR failed: {ex.Message}");
            return string.Empty;
        }
    }
}