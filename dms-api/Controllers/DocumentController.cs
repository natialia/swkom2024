using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory; // For creating HTTP clients
        private readonly IMapper _mapper; // For mapping DTOs to entities
        private readonly ILogger<DocumentController> _logger; // For logging

        public DocumentController(IHttpClientFactory httpClientFactory, IMapper mapper, ILogger<DocumentController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            var client = _httpClientFactory.CreateClient("dms-dal"); // Create a client for the DAL
            var response = await client.GetAsync("/api/document"); // Call the endpoint in the DAL

            if (response.IsSuccessStatusCode)
            {
                var items = await response.Content.ReadFromJsonAsync<IEnumerable<Document>>(); // Read documents from the response
                var dtoItems = _mapper.Map<IEnumerable<DocumentDTO>>(items); // Map entities to DTOs
                return Ok(dtoItems); // Return the mapped DTOs
            }

            return StatusCode((int)response.StatusCode, "Error retrieving documents from DAL");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(long id)
        {
            var client = _httpClientFactory.CreateClient("dms-dal");
            var response = await client.GetAsync($"/api/document/{id}");

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<Document>(); // Get the document
                if (item == null)
                {
                    return NotFound("Document not found"); // Return 404 if not found
                }

                var dtoItem = _mapper.Map<DocumentDTO>(item); // Map entity to DTO
                return Ok(dtoItem); // Return the DTO
            }

            return StatusCode((int)response.StatusCode, "Error retrieving document from DAL");
        }

        [HttpPost]
        public async Task<IActionResult> PostDocument(DocumentDTO documentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 for invalid model state
            }

            var client = _httpClientFactory.CreateClient("dms-dal");
            var document = _mapper.Map<Document>(documentDto); // Map DTO to entity
            var response = await client.PostAsJsonAsync("/api/document", document); // Send POST request to DAL

            if (response.IsSuccessStatusCode)
            {
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, documentDto); // Return 201 Created
            }

            return StatusCode((int)response.StatusCode, "Error creating document in DAL");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(long id, DocumentDTO documentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return 400 for invalid model state
            }

            if (id != documentDto.Id)
            {
                return BadRequest("ID mismatch"); // Ensure ID matches
            }

            var client = _httpClientFactory.CreateClient("dms-dal");
            var document = _mapper.Map<Document>(documentDto); // Map DTO to entity
            var response = await client.PutAsJsonAsync($"/api/document/{id}", document); // Send PUT request to DAL

            if (response.IsSuccessStatusCode)
            {
                return NoContent(); // Return 204 No Content
            }

            return StatusCode((int)response.StatusCode, "Error updating document in DAL");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(long id)
        {
            var client = _httpClientFactory.CreateClient("dms-dal");
            var response = await client.DeleteAsync($"/api/document/{id}"); // Send DELETE request to DAL

            if (response.IsSuccessStatusCode)
            {
                return NoContent(); // Return 204 No Content
            }

            return StatusCode((int)response.StatusCode, "Error deleting document in DAL");
        }
    }
}