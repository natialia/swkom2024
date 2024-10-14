using Microsoft.AspNetCore.Mvc;

namespace dms_dal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentItemsController : ControllerBase
    {

        private readonly ILogger<DocumentItemsController> _logger;

        public DocumentItemsController(ILogger<DocumentItemsController> logger)
        {
            _logger = logger;
        }
    }
}
