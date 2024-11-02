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
    public class DocumentController : ControllerBase
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
            var documents = await _documentService.GetAllDocumentsAsync();
            var dtos = _mapper.Map<IEnumerable<DocumentDTO>>(documents);

            return Ok(dtos); //Return all documents
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var item = await _documentService.GetDocumentByIdAsync(id); // Get the document
            if (item == null)
            {
                return NotFound("Document not found"); // Return 404 if not found
            }

            var dtoItem = _mapper.Map<DocumentDTO>(item); // Map entity to DTO
            return Ok(dtoItem); // Return the DTO
        }

        [HttpPost]
        public async Task<IActionResult> PostDocument(DocumentDTO documentDto)
        {
            var document = _mapper.Map<Document>(documentDto); // Map DTO to Document

            var result = await _documentService.AddDocumentAsync(document);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, documentDto); // Return 201 Created
            }

            return StatusCode(400, result.Message); //400 Bad request, post unsuccessful

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, DocumentDTO documentDto)
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var response = await _documentService.DeleteAsync(id); // Send DELETE request to BL

            if (response.Success)
            {
                return NoContent(); // Return 204 No Content
            }

            return StatusCode(400, response.Message); //return 400 Bad Request
        }
    }
}