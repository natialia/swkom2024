using Microsoft.AspNetCore.Mvc;
using dms_dal.Repositories;
using dms_dal.Entities;

namespace dms_dal.Controllers
{
    public class DocumentItemController
    {
        [ApiController]
        [Route("api/[controller]")]
        public class TodoController(IDocumentItemRepository repository) : ControllerBase
        {
            [HttpGet]
            public async Task<IEnumerable<DocumentItem>> GetAsync()
            {
                return await repository.GetAllAsync();
            }

            [HttpPost]
            public async Task<IActionResult> PostAsync(DocumentItem item)
            {
                if (string.IsNullOrWhiteSpace(item.Name))
                {
                    return BadRequest(new { message = "Task name cannot be empty." });
                }
                await repository.AddAsync(item);
                return Ok();
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> PutAsync(int id, DocumentItem item)
            {
                var existingItem = await repository.GetByIdAsync(id);
                if (existingItem == null)
                {
                    return NotFound();
                }

                existingItem.Name = item.Name;
                existingItem.IsComplete = item.IsComplete;
                await repository.UpdateAsync(existingItem);
                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteAsync(int id)
            {
                var item = await repository.GetByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                await repository.DeleteAsync(id);
                return NoContent();
            }
        }
    }
}
