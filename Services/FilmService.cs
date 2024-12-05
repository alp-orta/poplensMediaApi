using TMDbLib.Client;
using TMDbLib.Objects.Movies;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using poplensMediaApi.Data;
using poplensMediaApi.Models;
using poplensMediaApi.Contracts;
using Microsoft.EntityFrameworkCore;

namespace poplensMediaApi.Services {
    public class FilmService : IFilmService {
        private readonly MediaDbContext _context;
        private readonly TMDbClient _tmdbClient;

        public FilmService(MediaDbContext context) {
            _context = context;
            _tmdbClient = new TMDbClient("9b0c587894f38101b0077b9e58e9c836"); // Replace with your actual TMDb API key
        }

        public async Task<int> FetchAndStorePopularMoviesAsync(int totalPages = 5) {
            var genres = await _tmdbClient.GetMovieGenresAsync(); // Fetch genres once and cache them
            var moviesToInsert = new List<Media>();
            int newMoviesInserted = 0;

            for (int page = 1; page <= totalPages; page++) {
                try {
                    var popularMovies = await _tmdbClient.GetMovieTopRatedListAsync(page: page);

                    foreach (var movie in popularMovies.Results) {
                        // Check if the movie already exists in the database
                        bool movieExists = await _context.Media.AnyAsync(m => m.CachedExternalId == movie.Id.ToString());
                        if (movieExists) continue;

                        // Convert GenreIds to Genre Names
                        var genreNames = movie.GenreIds
                            .Select(id => genres.FirstOrDefault(g => g.Id == id)?.Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList();

                        // Fetch movie credits (for director)
                        var credits = await _tmdbClient.GetMovieCreditsAsync(movie.Id);
                        var director = credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;

                        var media = new Media {
                            Id = Guid.NewGuid(),
                            Title = movie.Title,
                            PublishDate = movie.ReleaseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                            Genre = string.Join(", ", genreNames),
                            CachedExternalId = movie.Id.ToString(), // TMDb movie ID
                            CachedImagePath = movie.PosterPath,
                            AvgRating = 0,
                            TotalReviews = 0, 
                            Description = movie.Overview,
                            Director = director ?? "Unknown",
                            Type = "film",
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow
                        };

                        moviesToInsert.Add(media);
                    }

                    // Batch insert for the current page
                    if (moviesToInsert.Any()) {
                        newMoviesInserted += moviesToInsert.Count;
                        await _context.Media.AddRangeAsync(moviesToInsert);
                        await _context.SaveChangesAsync();
                        moviesToInsert.Clear(); // Clear the list for the next batch
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"Error processing page {page}: {ex.Message}");
                    // Optionally, log the error and continue to the next page
                }
            }

            return newMoviesInserted;
        }

        public async Task<List<Media>> FetchMoviesByGenreAsync(int genreId, int totalPages = 5) {
            var moviesToInsert = new List<Media>();
            var genres = await _tmdbClient.GetMovieGenresAsync();

            for (int page = 1; page <= totalPages; page++) {
                var movies = await _tmdbClient.DiscoverMoviesAsync()
                    .IncludeWithAllOfGenre(new List<int> { genreId})
                    .Query(page);

                foreach (var movie in movies.Results) {
                    if (await _context.Media.AnyAsync(m => m.CachedExternalId == movie.Id.ToString())) continue;

                    var genreNames = movie.GenreIds
                        .Select(id => genres.FirstOrDefault(g => g.Id == id)?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    var credits = await _tmdbClient.GetMovieCreditsAsync(movie.Id);
                    var director = credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;

                    var media = new Media {
                        Id = Guid.NewGuid(),
                        Title = movie.Title,
                        PublishDate = movie.ReleaseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                        Genre = string.Join(", ", genreNames),
                        CachedExternalId = movie.Id.ToString(),
                        CachedImagePath = movie.PosterPath,
                        AvgRating = 0,
                        TotalReviews = 0,
                        Description = movie.Overview,
                        Director = director ?? "Unknown",
                        Type = "film",
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    };
                    if (media.CachedImagePath != null) {
                        moviesToInsert.Add(media);
                    }
                }

                await _context.Media.AddRangeAsync(moviesToInsert);
                await _context.SaveChangesAsync();
                moviesToInsert.Clear();
            }

            return moviesToInsert;
        }

        public async Task<int> FetchMoviesByReleaseYearAsync(int year, int totalPages = 5) {
            var moviesToInsert = new List<Media>();
            int newInserted = 0;
            var genres = await _tmdbClient.GetMovieGenresAsync();

            // Fetch all existing CachedExternalIds to avoid repetitive database checks
            var existingMovieIds = await _context.Media
                .Where(m => m.Type == "film")
                .Select(m => m.CachedExternalId)
                .ToListAsync();

            for (int page = 1; page <= totalPages; page++) {
                try {
                    var movies = await _tmdbClient.DiscoverMoviesAsync()
                        .WherePrimaryReleaseIsInYear(year)
                        .Query(page);

                    foreach (var movie in movies.Results) {
                        // Check for duplicates before adding to the list
                        if (existingMovieIds.Contains(movie.Id.ToString())) continue;

                        // Convert genre IDs to genre names
                        var genreNames = movie.GenreIds
                            .Select(id => genres.FirstOrDefault(g => g.Id == id)?.Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .ToList();

                        // Fetch director information
                        var credits = await _tmdbClient.GetMovieCreditsAsync(movie.Id);
                        var director = credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;

                        // Create media entity
                        var media = new Media {
                            Id = Guid.NewGuid(),
                            Title = movie.Title,
                            PublishDate = movie.ReleaseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                            Genre = string.Join(", ", genreNames),
                            CachedExternalId = movie.Id.ToString(),
                            CachedImagePath = movie.PosterPath,
                            AvgRating = 0,
                            TotalReviews = 0,
                            Description = movie.Overview,
                            Director = director ?? "Unknown",
                            Type = "film",
                            CreatedDate = DateTime.UtcNow,
                            LastUpdatedDate = DateTime.UtcNow
                        };
                        if (media.CachedImagePath != null) {
                            moviesToInsert.Add(media);
                        }
                    }

                    if (moviesToInsert.Any()) {
                        await _context.Media.AddRangeAsync(moviesToInsert);
                        await _context.SaveChangesAsync();
                        newInserted += moviesToInsert.Count;
                        moviesToInsert.Clear(); // Clear only after saving
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"Error fetching movies for year {year}, page {page}: {ex.Message}");
                }
            }

            return newInserted;
        }


        public async Task<Dictionary<string, int>> FetchMoviesForAllYearsAsync() {
            int startYear = 1950;
            int endYear = 2024;
            int pagesPerYear = 15;
            var yearCount = new Dictionary<string, int>();

            for (int year = startYear; year <= endYear; year++) {
                try {
                    Console.WriteLine($"Fetching movies for year: {year}");
                    var count = await FetchMoviesByReleaseYearAsync(year, pagesPerYear);
                    yearCount[year.ToString()] = count;
                    Console.WriteLine($"Year {year}: {count} movies added.");
                } catch (Exception ex) {
                    Console.WriteLine($"Error fetching movies for year {year}: {ex.Message}");
                }
            }

            Console.WriteLine("Finished fetching movies from 1950 to 2024.");
            return yearCount;
        }



        public async Task<List<Media>> FetchMoviesByLanguageAsync(string languageCode, int totalPages = 5) {
            var moviesToInsert = new List<Media>();
            var allInserted = new List<Media>();
            var genres = await _tmdbClient.GetMovieGenresAsync();

            for (int page = 1; page <= totalPages; page++) {
                var movies = await _tmdbClient.DiscoverMoviesAsync()
                    .WhereOriginalLanguageIs(languageCode)
                    .Query(page);

                foreach (var movie in movies.Results) {
                    if (await _context.Media.AnyAsync(m => m.CachedExternalId == movie.Id.ToString())) continue;

                    var genreNames = movie.GenreIds
                        .Select(id => genres.FirstOrDefault(g => g.Id == id)?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    var credits = await _tmdbClient.GetMovieCreditsAsync(movie.Id);
                    var director = credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;

                    var media = new Media {
                        Id = Guid.NewGuid(),
                        Title = movie.Title,
                        PublishDate = movie.ReleaseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                        Genre = string.Join(", ", genreNames),
                        CachedExternalId = movie.Id.ToString(),
                        CachedImagePath = movie.PosterPath,
                        AvgRating = 0,
                        TotalReviews = 0,
                        Description = movie.Overview,
                        Director = director ?? "Unknown",
                        Type = "film",
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    };
                    if (media.CachedImagePath != null) {
                        moviesToInsert.Add(media);
                    }
                }
                allInserted.AddRange(moviesToInsert);
                await _context.Media.AddRangeAsync(moviesToInsert);
                await _context.SaveChangesAsync();
                moviesToInsert.Clear();
            }

            return allInserted;
        }

        public async Task<List<Media>> FetchTopRatedMoviesByCountryAsync(string countryCode, int totalPages = 5) {
            var moviesToInsert = new List<Media>();
            var genres = await _tmdbClient.GetMovieGenresAsync();

            for (int page = 1; page <= totalPages; page++) {
                var movies = await _tmdbClient.DiscoverMoviesAsync()
                    .WhereReleaseDateIsInRegion(countryCode)
                    .Query(page);

                foreach (var movie in movies.Results) {
                    if (await _context.Media.AnyAsync(m => m.CachedExternalId == movie.Id.ToString())) continue;

                    var genreNames = movie.GenreIds
                        .Select(id => genres.FirstOrDefault(g => g.Id == id)?.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    var credits = await _tmdbClient.GetMovieCreditsAsync(movie.Id);
                    var director = credits.Crew.FirstOrDefault(c => c.Job == "Director")?.Name;

                    var media = new Media {
                        Id = Guid.NewGuid(),
                        Title = movie.Title,
                        PublishDate = movie.ReleaseDate?.ToUniversalTime() ?? DateTime.UtcNow,
                        Genre = string.Join(", ", genreNames),
                        CachedExternalId = movie.Id.ToString(),
                        CachedImagePath = movie.PosterPath,
                        AvgRating = 0,
                        TotalReviews = 0,
                        Description = movie.Overview,
                        Director = director ?? "Unknown",
                        Type = "film",
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    };
                    if (media.CachedImagePath != null) {
                        moviesToInsert.Add(media);
                    }
                }

                await _context.Media.AddRangeAsync(moviesToInsert);
                await _context.SaveChangesAsync();
                moviesToInsert.Clear();
            }

            return moviesToInsert;
        }


    }

}
