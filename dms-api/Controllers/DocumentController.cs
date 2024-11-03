using DocumentManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using dms_bl.Services;
using dms_bl.Models;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Exceptions;
using DocumentManagementSystem.Exceptions;
using DocumentManagementSystem.Exceptions.Messaging;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase, IDisposable
    {
        private readonly IMapper _mapper; // For mapping DTOs to entities
        private readonly ILogger<DocumentController> _logger; // For logging
        private readonly IDocumentService _documentService; // Service for document operations
        private readonly IConnection _connection; // RabbitMQ connection
        private readonly IModel _channel; // RabbitMQ channel

        public DocumentController(IMapper mapper, ILogger<DocumentController> logger, IDocumentService documentService)
        {
            _mapper = mapper; // Initialize the mapper
            _logger = logger; // Initialize the logger
            _documentService = documentService; // Initialize the document service

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

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            _logger.LogInformation("Retrieving all documents...");
            try
            {
                // Fetch all documents from the service
                var documents = await _documentService.GetAllDocumentsAsync();
                var dtos = _mapper.Map<IEnumerable<DocumentDTO>>(documents); // Map entities to DTOs
                _logger.LogInformation("Successfully retrieved all documents.");
                return Ok(dtos); // Return the mapped DTOs
            }
            catch (Exception ex)
            {
                // Log errors encountered while retrieving documents
                _logger.LogError("Error while retrieving documents: {Exception}", ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            _logger.LogInformation("Retrieving document with ID {Id}...", id);
            try
            {
                // Attempt to retrieve a specific document by ID
                var item = await _documentService.GetDocumentByIdAsync(id);
                if (item == null)
                {
                    _logger.LogWarning("Document with ID {Id} not found.", id);
                    return NotFound("Document not found"); // Return 404 if not found
                }

                var dtoItem = _mapper.Map<DocumentDTO>(item); // Map entity to DTO
                _logger.LogInformation("Successfully retrieved document with ID {Id}.", id);
                return Ok(dtoItem); // Return the mapped DTO
            }
            catch (Exception ex)
            {
                // Log errors encountered while retrieving the document
                _logger.LogError("Error while retrieving document with ID {Id}: {Exception}", id, ex);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostDocument(DocumentDTO documentDto)
        {
            _logger.LogInformation("Attempting to create a new document...");
            try
            {
                var document = _mapper.Map<Document>(documentDto); // Map DTO to Document entity
                var result = await _documentService.AddDocumentAsync(document); // Add document via service

                if (result.Success)
                {
                    // Log success and send message to RabbitMQ
                    _logger.LogInformation("Document created successfully with ID {DocumentId}.", document.Id);
                    SendToMessageQueue(document.Name); // Send the document name to RabbitMQ
                    return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, documentDto); // Return 201 Created
                }

                // Log validation failure
                _logger.LogWarning("Document validation failed: {Message}", result.Message);
                return StatusCode(400, result.Message); // Return 400 Bad Request
            }
            catch (RabbitMQClientException ex)
            {
                // Log errors related to RabbitMQ client
                _logger.LogError("Failed to send message to RabbitMQ: {Exception}", ex);
                return StatusCode(500, "Error connecting to RabbitMQ");
            }
            catch (Exception ex)
            {
                // Log other unexpected errors
                _logger.LogError("An error occurred while creating the document: {Exception}", ex);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, DocumentDTO documentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Return 400 for invalid model state
                }

                if (id != documentDto.Id)
                {
                    return BadRequest("ID mismatch"); // Ensure ID matches
                }

                var document = _mapper.Map<Document>(documentDto); // Map DTO to document
                var response = await _documentService.UpdateDocumentAsync(id, document); // Update document via service

                if (response.Success)
                {
                    return NoContent(); // Return 204 No Content
                }

                return StatusCode(400, response.Message); // Return 400 Bad Request
            }
            catch (Exception ex)
            {
                // Log errors during document update
                _logger.LogError("An error occurred while creating the document: {Exception}", ex);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                // Send DELETE request to the service to remove the document
                var response = await _documentService.DeleteAsync(id);

                if (response.Success)
                {
                    return NoContent(); // Return 204 No Content
                }

                return StatusCode(400, response.Message); // Return 400 Bad Request
            }
            catch (Exception ex)
            {
                // Log errors during document deletion
                _logger.LogError("An error occurred while creating the document: {Exception}", ex);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        private void SendToMessageQueue(string fileName)
        {
            // Message Queue Logging
            try
            {
                _logger.LogInformation("Sending document {FileName} to RabbitMQ queue...", fileName);
                var body = Encoding.UTF8.GetBytes(fileName); // Convert file name to byte array
                _channel.BasicPublish(exchange: "", routingKey: "document_queue", basicProperties: null, body: body); // Publish message
                _logger.LogInformation("Document {FileName} successfully sent to queue.", fileName);
            }
            catch (Exception ex)
            {
                // Log errors encountered while sending to RabbitMQ
                _logger.LogError("Error sending document {FileName} to RabbitMQ queue: {Exception}", fileName, ex);
                throw new QueueException("Error sending message to RabbitMQ.", ex);
            }
        }

        public void Dispose()
        {
            // Dispose Logging
            _logger.LogInformation("Disposing RabbitMQ resources...");
            _channel?.Close(); // Close the channel
            _connection?.Close(); // Close the connection
            _logger.LogInformation("RabbitMQ resources disposed."); // Log disposal completion
        }
    }
}
