using Microsoft.AspNetCore.Mvc;
using poplensMediaApi.Contracts;

namespace poplensMediaApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class BookController : ControllerBase {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService) {
            _bookService = bookService;
        }

        // Fetch Popular Books - API Endpoint
        [HttpGet("FetchPopularBooks")]
        public async Task<IActionResult> FetchPopularBooks(string subject, int limit = 40, int offset = 0) {
            try {
                // Call the BookService to fetch and store popular books
                var newBooksInserted = await _bookService.FetchPopularBooksAsync(subject, limit, offset);

                // Return success response with the number of new books inserted
                return Ok(new { Message = $"{newBooksInserted} popular books inserted successfully." });
            } catch (System.Exception ex) {
                // Handle any exceptions and return an error response
                return StatusCode(500, new { Message = $"An error occurred while fetching books: {ex.Message}" });
            }
        }
    }
}
