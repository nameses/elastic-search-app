using Microsoft.AspNetCore.Mvc;

namespace elastic_search_app.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class BookController : Controller
    {
        private readonly ILogger<BookController> _logger;

        public BookController(ILogger<BookController> logger)
        {
            _logger=logger;
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetById(id);

            if (user==null) return NotFound();

            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> Create()
        {

        }
    }
}
