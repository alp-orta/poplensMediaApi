using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using poplensMediaApi.Contracts;
using poplensMediaApi.Models;

namespace poplensMediaApi.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService) {
            _mediaService = mediaService;
        }

        // Add a new media item
        [HttpPost]
        public async Task<IActionResult> CreateMedia([FromBody] Media media) {
            if (media == null) return BadRequest();
            if (!IsValidType(media.Type)) return BadRequest("Invalid media type. Must be 'film', 'book', or 'game'.");

            var createdMedia = await _mediaService.CreateMedia(media);
            return CreatedAtAction(nameof(GetMediaById), new { id = createdMedia.Id }, createdMedia);
        }

        // Get all media items
        [HttpGet]
        public async Task<IActionResult> GetAllMedia() {
            var mediaList = await _mediaService.GetAllMedia();
            return Ok(mediaList);
        }

        // Get a single media item by ID
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMediaById(Guid id) {
            var media = await _mediaService.GetMediaById(id);
            if (media == null) return NotFound();
            return Ok(media);
        }

        // Update an existing media item
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedia(Guid id, [FromBody] Media updatedMedia) {
            if (!IsValidType(updatedMedia.Type)) return BadRequest("Invalid media type. Must be 'film', 'book', or 'game'.");

            var isUpdated = await _mediaService.UpdateMedia(id, updatedMedia);
            if (!isUpdated) return NotFound();
            return NoContent();
        }

        // Delete a media item by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedia(Guid id) {
            var isDeleted = await _mediaService.DeleteMedia(id);
            if (!isDeleted) return NotFound();
            return NoContent();
        }

        // Search for media by title or description
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("SearchMedia")]
        public async Task<IActionResult> SearchMedia([FromQuery] string query) {
            if (string.IsNullOrEmpty(query)) return BadRequest("Query parameter is required.");

            var mediaList = await _mediaService.SearchMedia(query);
            return Ok(mediaList);
        }

        // Search for films by title or description
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("SearchFilms")]
        public async Task<IActionResult> SearchFilms([FromQuery] string query) {
            if (string.IsNullOrEmpty(query)) return BadRequest("Query parameter is required.");

            var mediaList = await _mediaService.SearchFilms(query);
            return Ok(mediaList);
        }

        // Search for books by title or description
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("SearchBooks")]
        public async Task<IActionResult> SearchBooks([FromQuery] string query) {
            if (string.IsNullOrEmpty(query)) return BadRequest("Query parameter is required.");

            var mediaList = await _mediaService.SearchBooks(query);
            return Ok(mediaList);
        }

        // Search for games by title or description
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("SearchGames")]
        public async Task<IActionResult> SearchGames([FromQuery] string query) {
            if (string.IsNullOrEmpty(query)) return BadRequest("Query parameter is required.");

            var mediaList = await _mediaService.SearchGames(query);
            return Ok(mediaList);
        }

        private bool IsValidType(string type) {
            return type == "film" || type == "book" || type == "game";
        }
    }

}
