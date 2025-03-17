using Microsoft.AspNetCore.Mvc;
using poplensMediaApi.Contracts;

namespace poplensMediaApi.Controllers {
    public class FilmController : ControllerBase {
        private readonly IFilmService _filmService;

        public FilmController(IFilmService filmService) {
            _filmService = filmService;
        }

        /// <summary>
        /// Fetch and Store Popular Movies Async
        /// </summary>
        /// <returns></returns>
        [HttpPost("FetchAndStorePopularMoviesAsync")]
        public async Task<IActionResult> FetchAndStorePopularMoviesAsync() {
            var movies = await _filmService.FetchAndStorePopularMoviesAsync(150);
            return Ok(movies);
        }

        /// <summary>
        /// Fetch and store movies by genre.
        /// </summary>
        [HttpPost("FetchMoviesByGenre")]
        public async Task<IActionResult> FetchMoviesByGenre([FromQuery] int genreId, [FromQuery] int totalPages = 5) {
            if (genreId <= 0)
                return BadRequest("GenreId is required.");

            var movies = await _filmService.FetchMoviesByGenreAsync(genreId, totalPages);
            return Ok(new { Count = movies.Count, Movies = movies });
        }


        [HttpPost("FetchMoviesForAllYearsAsync")]
        public async Task<IActionResult> FetchMoviesForAllYearsAsync() {
            var yearCount = await _filmService.FetchMoviesForAllYearsAsync();
            return Ok(yearCount);
        }

        /// <summary>
        /// Fetch and store movies by release year.
        /// </summary>
        [HttpPost("FetchMoviesByReleaseYear")]
        public async Task<IActionResult> FetchMoviesByReleaseYear([FromQuery] int year, [FromQuery] int totalPages = 5) {
            if (year <= 0)
                return BadRequest("Invalid release year.");

            var count = await _filmService.FetchMoviesByReleaseYearAsync(year, totalPages);
            return Ok(new { Count = count });
        }

        /// <summary>
        /// Fetch and store movies by language.
        /// </summary>
        [HttpPost("FetchMoviesByLanguage")]
        public async Task<IActionResult> FetchMoviesByLanguage([FromQuery] string languageCode, [FromQuery] int totalPages = 5) {
            if (string.IsNullOrWhiteSpace(languageCode))
                return BadRequest("LanguageCode is required.");

            var movies = await _filmService.FetchMoviesByLanguageAsync(languageCode, totalPages);
            return Ok(new { Count = movies.Count, Movies = movies });
        }

        /// <summary>
        /// Fetch and store top-rated movies by country.
        /// </summary>
        [HttpPost("FetchTopRatedMoviesByCountry")]
        public async Task<IActionResult> FetchTopRatedMoviesByCountry([FromQuery] string countryCode, [FromQuery] int totalPages = 5) {
            if (string.IsNullOrWhiteSpace(countryCode))
                return BadRequest("CountryCode is required.");

            var movies = await _filmService.FetchTopRatedMoviesByCountryAsync(countryCode, totalPages);
            return Ok(new { Count = movies.Count, Movies = movies });
        }
    }

}
