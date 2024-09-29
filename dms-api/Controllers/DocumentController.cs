using DocumentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly ILogger<DocumentController> _logger;

        public DocumentController(ILogger<DocumentController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(long id)
        {
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
