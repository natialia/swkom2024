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
using dms_api.DTOs;
using Minio;
using Minio.DataModel.Args;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;


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
        private readonly IMinioClient _minioClient; // Minio
        private const string BucketName = "files";
        private readonly ElasticsearchClient _elasticClient; // ElasticSearch

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentController"/> class.
        /// </summary>
        /// <param name="mapper">Mapper for converting between DTOs and entities.</param>
        /// <param name="logger">Logger for recording actions and errors.</param>
        /// <param name="documentService">Service for document operations.</param>
        /// /// <param name="messageQueueService">Service for sending messages to rabbitmq queue.</param>
        public DocumentController(IMapper mapper, ILogger<DocumentController> logger, IDocumentLogic documentService, IMessageQueueService messageQueueService, ElasticsearchClient elasticClient)
        {
            _mapper = mapper; // Initialize the mapper
            _logger = logger; // Initialize the logger
            _documentService = documentService; // Initialize the document service
            _messageQueueService = messageQueueService; // Initialize message queue service
            _elasticClient = elasticClient;
            _minioClient = new MinioClient()
                .WithEndpoint("minio", 9000)
                .WithCredentials("minioadmin", "minioadmin")
                .WithSSL(false)
                .Build();
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

        //// Create Index for ElasticSearch
        //private async Task EnsureIndexExists()
        //{
        //    var existsResponse = await _elasticClient.Indices.ExistsAsync("documents");
        //    if (!existsResponse.Exists)
        //    {
        //        var createIndexResponse = await _elasticClient.Indices.CreateAsync("documents", c => c
        //            .Mappings(m => m
        //                .Properties(p =>
        //                {
        //                    p.Text(t => t.Name("Name"));        // Define Name as a full-text searchable field
        //                    p.Keyword(k => k.Name("FileType")); // Define FileType as a keyword for exact match
        //                    p.Keyword(k => k.Name("FileSize")); // Define FileSize as a keyword for exact match
        //                    p.Text(t => t.Name("OcrText"));     // Define OcrText as a full-text searchable field
        //                })
        //            )
        //        );

        //        if (!createIndexResponse.IsValidResponse)
        //        {
        //            throw new Exception($"Failed to create index: {createIndexResponse.DebugInformation}");
        //        }
        //    }
        //}


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
                    OcrText = request.OcrText
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
                    await EnsureBucketExists();

                    var fileName = Path.GetFileName(uploadedDocument.FileName);
                    await using var fileStream = uploadedDocument.OpenReadStream();

                    await _minioClient.PutObjectAsync(new PutObjectArgs()
                        .WithBucket(BucketName)
                        .WithObject(fileName)
                        .WithStreamData(fileStream)
                        .WithObjectSize(uploadedDocument.Length));
                    // Log success and send message to RabbitMQ
                    _logger.LogInformation("Document created successfully with ID {DocumentId}.", resultItem.Id);
                    _messageQueueService.SendToQueue($"{resultItem.Id}|{fileName}"); // Send the document Id to RabbitMQ for OcrWorker
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

        private async Task EnsureBucketExists()
        {
            _logger.LogInformation($"Looking for bucket {BucketName} ...");
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(BucketName));
            if (!found)
            {
                _logger.LogInformation($"Bucket {BucketName} not found, now creating new Bucket");
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(BucketName));
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

        [HttpGet("ocrText/{id}")]
        public async Task<IActionResult> GetTextOfDocument(int id)
        {

            var resultDocument = await _documentService.GetDocumentByIdAsync(id);
            if(resultDocument != null)
            {
                _logger.LogInformation($"Document with Id {id} found. Getting OCR Text");
                _logger.LogInformation($"OCR text is: {resultDocument.OcrText}");
                return StatusCode(200, resultDocument.OcrText);
            }

            _logger.LogWarning($"Document with Id {id} not found.");
            return StatusCode(400);
        }

        [HttpPut("ocrText/{id}")]
        public async Task<IActionResult> PutDocumentWithText(int id, Document document)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model state for ocr Update is invalid");
                    return BadRequest(ModelState); // Return 400 for invalid model state
                }

                if (id != document.Id)
                {
                    _logger.LogWarning("IDs do not match: sent id "+id+", document id: "+document.Id);
                    return BadRequest("ID mismatch"); // Ensure ID matches
                }

                var response = await _documentService.UpdateDocumentAsync(id, document); // Update document via service

                if (response.Success)
                {
                    _logger.LogInformation("Successfully added ocr text to document " + id);
                    return NoContent(); // Return 204 No Content
                }

                _logger.LogWarning("Ocr update was not possible: " + response.Message);
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

        /// <summary>
        /// Searches documents by query string.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>A list of matching documents.</returns>
        [HttpPost("search/querystring")]
        [HttpPost("search/querystring")]
        public async Task<IActionResult> SearchByQueryString([FromBody] string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "Search term cannot be empty" });
                }

                var response = await _elasticClient.SearchAsync<Document>(s => s
                    .Index("documents")
                    .Query(q => q.QueryString(qs => qs.Query($"*{searchTerm}*").Fields("OcrText")))); // Ensure the field matches your mapping

                if (!response.IsValidResponse)
                {
                    _logger.LogError("Elasticsearch error: {DebugInfo}", response.DebugInformation);
                    return StatusCode(500, new { message = "Elasticsearch error", details = response.DebugInformation });
                }

                if (!response.Documents.Any())
                {
                    return NotFound(new { message = "No documents found for the given search term." });
                }

                return Ok(response.Documents);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while searching documents: {Exception}", ex);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        [HttpGet("search/querystring")]
        public async Task<IActionResult> SearchByQueryStringGet(string searchTerm)
        {
            return await SearchByQueryString(searchTerm);
        }

        /// <summary>
        /// Searches documents by fuzzy search.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>A list of matching documents.</returns>
        [HttpPost("search/fuzzy")]
        public async Task<IActionResult> SearchByFuzzy([FromBody] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { message = "Search term cannot be empty" });
            }

            var response = await _elasticClient.SearchAsync<Document>(s => s
                .Index("documents")
                .Query(q => q.Match(m => m
                .Field("OcrText")
                .Query(searchTerm)
                .Fuzziness(new Fuzziness(4)))));

            return HandleSearchResponse(response);
        }

        private IActionResult HandleSearchResponse(SearchResponse<Document> response)
        {
            if (response.IsValidResponse)
            {
                if (response.Documents.Any())
                {
                    return Ok(response.Documents);
                }
                return NotFound(new { message = "No documents found matching the search term." });
            }

            return StatusCode(500, new { message = "Failed to search documents", details = response.DebugInformation });
        }

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
