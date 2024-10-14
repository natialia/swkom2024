using AutoMapper;
using DocumentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DocumentController> _logger;
        private readonly IMapper _mapper;

        public DocumentController(IHttpClientFactory httpClientFactory, ILogger<DocumentController> logger, IMapper mapper)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(long id)
        {
            var client = _httpClientFactory.CreateClient("dms-dal");
            var response = await client.GetAsync("/api/document"); // Endpunkt des DAL
            return Ok("Document was found");
        }

        [HttpPost]
        public async Task<IActionResult> PostDocument(Document document)
        {
            return Ok("document was created");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(long id, Document document)
        {
            return Ok("document was updated");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(long id)
        {
            return Ok("document was deleted");
        }

    }
}
