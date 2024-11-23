﻿using DocumentManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using dms_bl.Services;
using dms_bl.Models;
using RabbitMQ.Client;
using System.Text;
using RabbitMQ.Client.Exceptions;
using DocumentManagementSystem.Exceptions;
using DocumentManagementSystem.Exceptions.Messaging;
using dms_api.DTOs;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase, IDisposable
    {
        private readonly IMapper _mapper; // For mapping DTOs to entities
        private readonly ILogger<DocumentController> _logger; // For logging
        private readonly IDocumentLogic _documentService; // Service for document operations
        private readonly IMessageQueueService _messageQueueService;
        private readonly IConnection _connection; // RabbitMQ connection
        private readonly IModel _channel; // RabbitMQ channel

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentController"/> class.
        /// </summary>
        /// <param name="mapper">Mapper for converting between DTOs and entities.</param>
        /// <param name="logger">Logger for recording actions and errors.</param>
        /// <param name="documentService">Service for document operations.</param>
        /// /// <param name="messageQueueService">Service for sending messages to rabbitmq queue.</param>
        public DocumentController(IMapper mapper, ILogger<DocumentController> logger, IDocumentLogic documentService, IMessageQueueService messageQueueService)
        {
            _mapper = mapper; // Initialize the mapper
            _logger = logger; // Initialize the logger
            _documentService = documentService; // Initialize the document service
            _messageQueueService = messageQueueService; // Initialize message queue service
        }

        /// <summary>
        /// Retrieves all documents.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of documents.</returns>
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

        /// <summary>
        /// Retrieves a specific document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to retrieve.</param>
        /// <returns>An <see cref="IActionResult"/> containing the document if found, otherwise 404.</returns>
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

        /// <summary>
        /// Creates a new document.
        /// </summary>
        /// <param name="request">The request containing document data.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
        [HttpPost]
        public async Task<IActionResult> PostDocument([FromForm] DocumentRequest request)
        {
            _logger.LogInformation("Attempting to create a new document...");
            try
            {
                var uploadedDocument = request.UploadedDocument;
                var documentDto = new DocumentDTO
                {
                    Id = request.Id,
                    Name = request.Name,
                    FileType = request.FileType,
                    FileSize = request.FileSize
                };

                if (uploadedDocument == null || uploadedDocument.Length == 0) //Receive uploaded file: use for document storage later
                {
                    _logger.LogWarning("No file uploaded.");
                    return BadRequest("A file needs to be uploaded.");
                }
                var document = _mapper.Map<Document>(documentDto); // Map DTO to Document entity
                var resultItem = await _documentService.AddDocumentAsync(document); // Add document via service

                if (resultItem != null)
                {
                    // Log success and send message to RabbitMQ
                    _logger.LogInformation("Document created successfully with ID {DocumentId}.", resultItem.Id);
                    _messageQueueService.SendToQueue($"{resultItem.Id}"); // Send the document Id to RabbitMQ for OcrWorker
                    return CreatedAtAction(nameof(GetDocument), new { id = resultItem.Id }, resultItem); // Return 201 Created
                }

                // Log validation failure
                _logger.LogWarning("Document validation failed");
                return StatusCode(400); // Return 400 Bad Request
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

        /// <summary>
        /// Updates an existing document.
        /// </summary>
        /// <param name="id">The ID of the document to update.</param>
        /// <param name="documentDto">The updated document data.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
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

        [HttpPut("/ocrText/{id}")]
        public async Task<IActionResult> PutDocumentWithText(int id, Document document)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState); // Return 400 for invalid model state
                }

                if (id != document.Id)
                {
                    return BadRequest("ID mismatch"); // Ensure ID matches
                }

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

        /// <summary>
        /// Deletes a document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
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

        ///


        private void SendToMessageQueue(string fileName)
        {
            //TODO: move to business layer
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
