using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dms_bl.Exceptions;

namespace dms_bl.Services
{
    public class MessageQueueService : IMessageQueueService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger _logger;

        public MessageQueueService(ILogger<MessageQueueService> logger)
        {
            _logger = logger;

            // RabbitMQ Connection + Logging
            try
            {
                var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "user", Password = "password" };
                _logger.LogInformation("Attempting to connect to RabbitMQ...");

                // Establish the RabbitMQ connection
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare the queue for document messages
                _channel.QueueDeclare(queue: "document_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                _logger.LogInformation("Successfully connected to RabbitMQ and declared queue.");
            }
            catch (BrokerUnreachableException ex)
            {
                // Log critical error if connection to RabbitMQ fails
                _logger.LogCritical("Failed to connect to RabbitMQ: {Exception}", ex);
                throw new QueueException("Error establishing connection to RabbitMQ.", ex);
            }
            catch (Exception ex)
            {
                // Log unexpected errors during initialization
                _logger.LogCritical("Unexpected error initializing RabbitMQ: {Exception}", ex);
                throw new QueueException("An unexpected error occurred during RabbitMQ initialization.", ex);
            }
        }

        public void SendToQueue(string message)
        {
            //Send string to rabbitmq queue
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "document_queue", basicProperties: null, body: body);
            _logger.LogInformation($"Sent {message} to queue");
        }

        public void Dispose()
        {
            if (_channel.IsOpen)
            {
                _channel.Close();
            }
            if (_connection.IsOpen)
            {
                _connection.Close();
            }
        }
    }
}
