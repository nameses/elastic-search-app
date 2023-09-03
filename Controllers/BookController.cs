using elastic_search_app.Entities;
using elastic_search_app.Services;
using Microsoft.AspNetCore.Mvc;

namespace elastic_search_app.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class BookController : Controller
    {
        private readonly ILogger<BookController> _logger;
        private readonly BookService _bookService;

        public BookController(ILogger<BookController> logger, BookService bookService)
        {
            _logger=logger;
            _bookService=bookService;
        }
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var user = await _bookService.GetByIdAsync(id);

            if (user==null) return NotFound();

            return Ok(user);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Book book)
        {
            var createdId = await _bookService.CreateAsync(book);

            if (createdId==null)
            {
                _logger.LogError($"Book was not created");
            }

            return Ok(createdId);
        }
        [HttpPost]
        [Route("generate/{amount}")]
        public async Task<IActionResult> Generate(int amount)
        {
            await _bookService.GenerateAsync(amount);

            return Ok();
        }
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Book book)
        {
            if (id != book.Id)
            {
                return BadRequest("Id mismatch between route parameter and request body.");
            }

            await _bookService.UpdateAsync(id, book);

            return NoContent(); // 204 No Content indicates a successful update with no response body
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var res = await _bookService.DeleteAsync(id);

            if (!res) return NotFound();

            return Ok();
        }
    }
}
