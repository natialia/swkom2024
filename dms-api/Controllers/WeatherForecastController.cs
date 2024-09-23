using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        // GET http://localhost:8081/
        [HttpGet]
        [Route("/")]
        public IActionResult GetHardcodedData()
        {
            // Hardcoded data
            var data = new
            {
                message = "Hello, this is hardcoded data!",
                status = 200
            };

            // Return 200 OK with JSON data
            return Ok(data);
        }
    }
}
