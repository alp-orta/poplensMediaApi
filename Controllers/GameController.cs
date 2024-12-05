using Microsoft.AspNetCore.Mvc;
using poplensMediaApi.Contracts;
using poplensMediaApi.Models;
using poplensMediaApi.Services;

namespace poplensMediaApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService) {
            _gameService = gameService;
        }

        /// <summary>
        /// Fetch and store games into the database.
        /// </summary>
        [HttpPost("FetchGames")]
        public async Task<IActionResult> FetchGames([FromBody] GameFetchRequest request) {
            if (request.PagesToFetch <= 0)
                return BadRequest("PagesToFetch must be greater than 0.");

            var insertedCount = await _gameService.FetchGamesAsync(request);

            if (insertedCount == 0)
                return Ok("No new games were added.");

            return Ok($"{insertedCount} games have been added successfully.");
        }
    }


}
