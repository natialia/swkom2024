using DocumentManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using dms_bl.Services;
using dms_bl.Models;
using RabbitMQ.Client; // Add RabbitMQ namespace
using System.Text;
using RabbitMQ.Client.Exceptions;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase, IDisposable
    {
        private readonly IMapper _mapper; // For mapping DTOs to entities
        private readonly ILogger<DocumentController> _logger; // For logging
        private readonly IDocumentService _documentService;
        private readonly IConnection _connection; // RabbitMQ connection
        private readonly IModel _channel; // RabbitMQ channel

        public DocumentController(IMapper mapper, ILogger<DocumentController> logger, IDocumentService documentService)
        {
            _mapper = mapper;
            _logger = logger;
            _documentService = documentService;

            // Connect to RabbitMQ
            var factory = new ConnectionFactory() { HostName = "rabbitmq", UserName = "user", Password = "password" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare Queue
            _channel.QueueDeclare(queue: "document_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            try
            {
                var documents = await _documentService.GetAllDocumentsAsync();
                var dtos = _mapper.Map<IEnumerable<DocumentDTO>>(documents);

                return Ok(dtos); //Return all documents
            }
            catch(Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving the document: {ex.Message}");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            try
            {
                var item = await _documentService.GetDocumentByIdAsync(id); // Get the document
                if (item == null)
                {
                    return NotFound("Document not found"); // Return 404 if not found
                }

                var dtoItem = _mapper.Map<DocumentDTO>(item); // Map entity to DTO
                return Ok(dtoItem); // Return the DTO
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while retrieving the document: {ex.Message}");
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostDocument(DocumentDTO documentDto)
        {
            try
            {
                var document = _mapper.Map<Document>(documentDto); // Map DTO to Document

                var result = await _documentService.AddDocumentAsync(document);

                if (result.Success)
                {
                    SendToMessageQueue(document.Name);
                    _logger.LogInformation($"Document created successfully with ID: {document.Id}");
                    return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, documentDto); // Return 201 Created
                }
                _logger.LogWarning($"Document validation failed: {result.Message}");
                return StatusCode(400, result.Message); //400 Bad request, post unsuccessful
            }
            catch(RabbitMQClientException ex)
            {
                _logger.LogError("Failed to send message to RabbitMQ: {Exception}", ex);
                return StatusCode(500, "Error connecting to RabbitMQ");
            }
            catch(Exception ex)
            {
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
                var response = await _documentService.UpdateDocumentAsync(id, document);

                if (response.Success)
                {
                    return NoContent(); // Return 204 No Content
                }

                return StatusCode(400, response.Message); //Return 400 Bad Request
            }
            catch(Exception ex)
            {
                _logger.LogError("An error occurred while creating the document: {Exception}", ex);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var response = await _documentService.DeleteAsync(id); // Send DELETE request to BL

                if (response.Success)
                {
                    return NoContent(); // Return 204 No Content
                }

                return StatusCode(400, response.Message); //return 400 Bad Request
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while creating the document: {Exception}", ex);
                return StatusCode(500, $"An internal server error occurred: {ex.Message}");
            }
        }

        private void SendToMessageQueue(string fileName)
        {
            // Send message to rabbitmq queue
            var body = Encoding.UTF8.GetBytes(fileName);
            _channel.BasicPublish(exchange: "", routingKey: "document_queue", basicProperties: null, body: body);
            Console.WriteLine($@"[x] Sent {fileName}");
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}