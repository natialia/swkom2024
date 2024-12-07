using AutoMapper;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dms_dal_new.Entities;
using Microsoft.Extensions.Logging;
using dms_bl.Exceptions;
using dms_dal_new.Repositories;
using System.Net.Http.Json;
using dms_bl.Models;

namespace dms_bl.Services
{
    public class RabbitMqListenerService: IHostedService
    {
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ConnectToRabbitMQ();
            StartListening();
            return Task.CompletedTask;
        }

        // With httpclient, because a hosted service cannot have scoped services (document logic, repository...) injected
        public RabbitMqListenerService(ILogger<RabbitMqListenerService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
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
                    _logger.LogInformation("Attempting to connect to RabbitMQ...");

                    // Establish the RabbitMQ connection
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    _channel.QueueDeclare(queue: "ocr_result_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                    _logger.LogInformation("Successfully connected to RabbitMQ and created result queue.");

                    break; // If connection works, leave loop for retries
                }
                catch (Exception ex)
                {
                    _logger.LogCritical("Unexpected error initializing RabbitMQ: {Exception} ... trying again in 5 seconds", ex);
                    Thread.Sleep(5000);
                    retries--;
                }
            }

            if (_connection == null || !_connection.IsOpen)
            {
                throw new QueueException("Connection was not possible, 5 tries failed.");
            }
        }

        //TODO: test if this works!!!!!!!!
        private void StartListening()
        {
            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var parts = message.Split('^'); //TODO: CHECK IF THERES A BETTER WAY THAN splitting with ^
                    _logger.LogInformation($"Received message: {message}");

                    if (parts.Length == 2)
                    {
                        var id = parts[0];
                        var extractedText = parts[1];
                        if (string.IsNullOrEmpty(extractedText))
                        {
                            _logger.LogError($@"Error: Empty OCR-Text for File {id}. Message will be ignored.");
                            return;
                        }

                        int numId;
                        bool success = int.TryParse(id, out numId);
                        if (success)
                        {
                            var client = _httpClientFactory.CreateClient("dms-api");
                            var response = await client.GetAsync($"/Document/{numId}"); //Get document 
                            if(!response.IsSuccessStatusCode)
                            {
                                _logger.LogError($"Document of result queue not found! {response.StatusCode} {response.Content}");
                                return;
                            }

                            _logger.LogInformation($"Document with id {id} was found.");

                            var documentItem = await response.Content.ReadFromJsonAsync<Document>();

                            if(documentItem != null)
                            {
                                _logger.LogInformation($"Document with id {id} was converted. Now adding {extractedText}");
                                documentItem.OcrText = extractedText;
                                var updateResponse = await client.PutAsJsonAsync($"/Document/ocrText/{id}", documentItem); //Update document with added ocr text
                                if (!updateResponse.IsSuccessStatusCode)
                                {
                                    _logger.LogError($"Could not add Ocr Text to Document with id {id}, update failed");
                                } else
                                {
                                    _logger.LogInformation($"Successfully updated and added Ocr Text to Document {id}");
                                }

                            } else
                            {
                                _logger.LogError($"Could not convert document with id {id}");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Error: Invalid message received.");
                    }
                };

                _channel.BasicConsume(queue: "ocr_result_queue", autoAck: true, consumer: consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error starting listener for OCR result queue: {ex.Message}");
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
