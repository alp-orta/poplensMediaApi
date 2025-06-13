using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pgvector;
using poplensMediaApi.Contracts;
using poplensMediaApi.Models;
using poplensMediaApi.Models.Common;
using poplensMediaApi.Services;

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

        // Get a single media item by ID without Embedding
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMediaById(Guid id) {
            var media = await _mediaService.GetMediaById(id);
            if (media == null) return NotFound();
            return Ok(media);
        }

        // Get a single media item by ID
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("GetMediaWithEmbeddingById/{id}")]
        public async Task<IActionResult> GetMediaWithEmbeddingById(Guid id) {
            var media = await _mediaService.GetMediaWithEmbeddingById(id);
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

        [HttpPost("IncrementTotalReviewCount/{id}")]
        public async Task<IActionResult> IncrementTotalReviewCount(Guid id) {
            var result = await _mediaService.IncrementTotalReviewCount(id);
            if (!result) {
                return NotFound(new { Message = "Media not found." });
            }
            return Ok(new { Message = "Review count incremented successfully." });
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

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("GetMediaWithFilters")]
        public async Task<IActionResult> GetMediaWithFilters(
            [FromQuery] string mediaType,
            [FromQuery] string? decade,
            [FromQuery] string? genre,
            [FromQuery] string? sortBy,
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) {

            var media = await _mediaService.GetMediaWithFilters(mediaType, decade, genre, sortBy, query, page, pageSize);
            var totalItems = await _mediaService.GetTotalMediaCount(mediaType, decade, genre, query);
            var pagedResult = new PageResult<Media> {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalItems,
                Result = media.ToList()
            };
            return Ok(pagedResult);
        }

        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpPost("Similar")]
        public async Task<IActionResult> GetSimilarMedia([FromBody] SimilarMediaRequest request) {
            if (request == null || request.Embedding == null || request.Embedding.Length != 384)
                return BadRequest("A valid 384-dimension embedding is required.");

            // Convert float[] to Vector
            var embedding = new Vector(new ReadOnlyMemory<float>(request.Embedding));

            var result = await _mediaService.GetSimilarMediaAsync(
                embedding,
                request.Count,
                request.MediaType,
                request.ExcludedMediaIds
            );

            return Ok(result);
        }


        [HttpPost("UpdateMissingMediaEmbeddings")]
        public async Task<IActionResult> UpdateMissingMediaEmbeddings() {
            var updatedCount = await _mediaService.UpdateMissingMediaEmbeddingsAsync();
            return Ok(new { UpdatedCount = updatedCount });
        }

        private bool IsValidType(string type) {
            return type == "film" || type == "book" || type == "game";
        }

    }

}
