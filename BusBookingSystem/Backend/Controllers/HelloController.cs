using Microsoft.AspNetCore.Mvc;

namespace BusBookingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelloController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetHello()
        {
            return Ok("Hello, backend is working 🚀");
        }
    }
}