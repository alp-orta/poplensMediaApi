using poplensMediaApi.Models;

namespace poplensMediaApi.Contracts {
    public interface IFilmService {
        Task<int> FetchAndStorePopularMoviesAsync(int totalPages);

        Task<List<Media>> FetchMoviesByGenreAsync(int genreId, int totalPages = 5);

        Task<int> FetchMoviesByReleaseYearAsync(int year, int totalPages = 5);

        Task<List<Media>> FetchMoviesByLanguageAsync(string languageCode, int totalPages = 5);

        Task<List<Media>> FetchTopRatedMoviesByCountryAsync(string countryCode, int totalPages = 5);

        Task<Dictionary<string, int>> FetchMoviesForAllYearsAsync();
    }
}
