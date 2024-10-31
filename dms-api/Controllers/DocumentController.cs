using DocumentManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using dms_dal.Entities;
using dms_dal.Data;
using dms_bl.Services;
using dms_bl.Models;
using Azure;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory; // For creating HTTP clients
        private readonly IMapper _mapper; // For mapping DTOs to entities
        private readonly ILogger<DocumentController> _logger; // For logging
        private readonly DocumentService _documentService;

        public DocumentController(IHttpClientFactory httpClientFactory, IMapper mapper, ILogger<DocumentController> logger,  DocumentService documentService)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _logger = logger;
            _documentService = documentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDocuments()
        {
            var documents = _documentService.GetAllDocumentsAsync();
            var dtos = _mapper.Map<IEnumerable<DocumentDTO>>(documents);

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            var client = _httpClientFactory.CreateClient("dms-dal");
            var response = await client.GetAsync($"/api/DocumentItem/{id}");

            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<DocumentItem>(); // Get the document
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
            var document = _mapper.Map<Document>(documentDto); // Map DTO to Document

            var result = await _documentService.AddDocumentAsync(document);

            if (result)
            {
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, documentDto); // Return 201 Created
            }

            return StatusCode(TODO, "Error creating document in DAL");

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

            var client = _httpClientFactory.CreateClient("dms-dal");
            var document = _mapper.Map<DocumentItem>(documentDto); // Map DTO to entity
            var response = await client.PutAsJsonAsync($"/api/DocumentItem/{id}", document); // Send PUT request to DAL

            if (response.IsSuccessStatusCode)
            {
                return NoContent(); // Return 204 No Content
            }

            return StatusCode((int)response.StatusCode, "Error updating document in DAL");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var client = _httpClientFactory.CreateClient("dms-dal");
            var response = await client.DeleteAsync($"/api/DocumentItem/{id}"); // Send DELETE request to DAL

            if (response.IsSuccessStatusCode)
            {
                return NoContent(); // Return 204 No Content
            }

            return StatusCode((int)response.StatusCode, "Error deleting document in DAL");
        }
    }
}