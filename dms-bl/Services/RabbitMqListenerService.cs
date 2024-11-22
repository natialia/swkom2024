using AutoMapper;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dms_dal_new.Entities;

namespace dms_bl.Services
{
    public class RabbitMqListenerService: IHostedService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly IHttpClientFactory _httpClientFactory;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ConnectToRabbitMQ();
            StartListening();
            return Task.CompletedTask;
        }

        public RabbitMqListenerService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

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

                    _channel.QueueDeclare(queue: "ocr_result_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    Console.WriteLine("Erfolgreich mit RabbitMQ verbunden und Queue erstellt.");

                    break; // Wenn die Verbindung klappt, verlässt es die Schleife
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Verbinden mit RabbitMQ: {ex.Message}. Versuche es in 5 Sekunden erneut...");
                    Thread.Sleep(5000);
                    retries--;
                }
            }

            if (_connection == null || !_connection.IsOpen)
            {
                throw new Exception("Konnte keine Verbindung zu RabbitMQ herstellen, alle Versuche fehlgeschlagen.");
            }
        }

        private void StartListening()
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var parts = message.Split('|');

                    Console.WriteLine($@"[Listener] Nachricht erhalten: {message}");

                    if (parts.Length == 2)
                    {
                        var id = parts[0];
                        var extractedText = parts[1];
                        if (string.IsNullOrEmpty(extractedText))
                        {
                            Console.WriteLine($@"Fehler: Leerer OCR-Text für Task {id}. Nachricht wird ignoriert.");
                            return;
                        }

                        var client = _httpClientFactory.CreateClient("TodoDAL");
                        var response = await client.GetAsync($"/api/todo/{id}");

                        if (response.IsSuccessStatusCode)
                        {
                            var todoItem = await response.Content.ReadFromJsonAsync<TodoItem>();
                            if (todoItem != null)
                            {
                                Console.WriteLine($@"[Listener] Task {id} erfolgreich abgerufen.");
                                Console.WriteLine($@"[Listener] OCR Text für Task {id}: {extractedText}");
                                Console.WriteLine($@"[Listener] Task vor Update: {todoItem}");

                                todoItem.OcrText = extractedText;

                                var updateResponse = await client.PutAsJsonAsync($"/api/todo/{id}", todoItem);
                                if (!updateResponse.IsSuccessStatusCode)
                                {
                                    Console.WriteLine($@"Fehler beim Aktualisieren des Tasks mit ID {id}");
                                }
                                else
                                {
                                    Console.WriteLine($@"OCR Text für Task {id} erfolgreich aktualisiert.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($@"[Listener] Task {id} nicht gefunden.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($@"Fehler beim Abrufen des Tasks mit ID {id}: {response.StatusCode}");
                        }
                    }
                    else
                    {
                        Console.WriteLine(@"Fehler: Ungültige Nachricht empfangen.");
                    }
                };

                _channel.BasicConsume(queue: "ocr_result_queue", autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Fehler beim Starten des Listeners für OCR-Ergebnisse: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel?.Close();
            _connection?.Close();
            return Task.CompletedTask;
        }
    }
}
