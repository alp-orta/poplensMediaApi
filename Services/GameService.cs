using poplensMediaApi.Data;
using IGDB;
using IGDB.Models;
using Microsoft.EntityFrameworkCore;
using poplensMediaApi.Models;
using poplensMediaApi.Contracts;
using RestEase.Implementation;
using TMDbLib.Objects.Movies;
using Microsoft.AspNetCore.Mvc.RazorPages;
using poplensMediaApi.Models.Game;
namespace poplensMediaApi.Services
{
    public class GameService : IGameService {
        private readonly IGDBClient _igdbClient;
        private readonly MediaDbContext _context;

        public GameService(MediaDbContext context) {
            _igdbClient = new IGDBClient("7ggnncnbidv2kboz18m5p0tgpr1o5c", "ldajb2z5anralfd2n3isowkg3ohdf2");
            _context = context;
        }

        public async Task<int> FetchGamesAsync(GameFetchRequest request) {
            var gamesToInsert = new List<Media>();
            int newInserted = 0;

            // Fetch all existing CachedExternalIds to avoid repetitive database checks
            var existingGameIds = await _context.Media
                .Where(m => m.Type == "game")
                .Select(m => m.CachedExternalId)
                .ToListAsync();

            for (int page = 0; page < request.PagesToFetch; page++) {
                var games = await _igdbClient.QueryAsync<Game>(
                    IGDBClient.Endpoints.Games,
                    query: $"fields id, name, first_release_date, genres.name, involved_companies.company.name, involved_companies.publisher, summary, cover.url; " +
                           $"where first_release_date != null & genres != null; " +
                           $"{(request.ReleaseYear > 0
                                ? $"where first_release_date >= {new DateTimeOffset(new DateTime(request.ReleaseYear.Value, 1, 1)).ToUnixTimeSeconds()} & first_release_date < {new DateTimeOffset(new DateTime(request.ReleaseYear.Value + 1, 1, 1)).ToUnixTimeSeconds()};"
                                : "")}" +
                            $"{(!string.IsNullOrWhiteSpace(request.Genre) ? $"& genres.name ~ \"{request.Genre}\";" : "")}" +
                           $"{(!string.IsNullOrWhiteSpace(request.Publisher) ? $"& involved_companies.company.name ~ \"{request.Publisher}\";" : "")}" +
                           $"limit 50; offset {page * 50};");

                foreach (var game in games) {
                    // Check for duplicates
                    if (existingGameIds.Contains(game.Id.ToString())) continue;

                    // Convert genres
                    var genreNames = game.Genres.Values?
                        .Select(g => g.Name) // Check if it's a full object and extract Name
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();


                    // Extract publisher
                    var publisher = game.InvolvedCompanies?.Values?
                        .FirstOrDefault(ic => ic.Company != null && ic.Publisher == true)?.Company?.Value;

                    // Create Media object
                    var media = new Media {
                        Id = Guid.NewGuid(),
                        Title = game.Name,
                        PublishDate = game.FirstReleaseDate.HasValue
                            ? (game.FirstReleaseDate.Value).UtcDateTime
                            : DateTime.UtcNow,

                        Genre = genreNames != null ? string.Join(", ", genreNames) : "Unknown",
                        CachedExternalId = game.Id.ToString(),
                        CachedImagePath = game.Cover?.Value.Url,
                        AvgRating = 0,
                        TotalReviews = 0,
                        Description = game.Summary ?? "No description available.",
                        Director = null,
                        Publisher = publisher?.Name ?? "Unknown",
                        Type = "game",
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    };
                    if (media.CachedImagePath != null) {
                        gamesToInsert.Add(media);
                    }
                }
            }
            // Insert into database
            newInserted += gamesToInsert.Count;
            await _context.Media.AddRangeAsync(gamesToInsert);
            await _context.SaveChangesAsync();

            return newInserted;
        }

        public async Task<int> FetchPopularGamesAsync(int limit = 100, int offset = 0) {

            // IGDB Query for popular games
            var query =
    $"fields id, name, first_release_date, genres.name, " +
    $"involved_companies.company.name, involved_companies.publisher, " +
    $"summary, cover.image_id, rating, rating_count; " +
    $"where rating > 75 & rating_count > 50 & first_release_date != null & genres != null; " +
    $"sort rating_count desc; " +
    $"limit {limit}; " +
    $"offset {offset};";




            try {
                // Execute the query
                var popularGames = await _igdbClient.QueryAsync<Game>("games", query);
                if (popularGames == null || popularGames.Length == 0) {
                    Console.WriteLine("No popular games found.");
                    return 0;
                }

                // Fetch existing game IDs
                var existingGameIds = await _context.Media
                    .Where(m => m.Type == "game")
                    .Select(m => m.CachedExternalId)
                    .ToListAsync();

                // Prepare media objects for new games
                var mediaList = new List<Media>();
                foreach (var game in popularGames) {
                    if (existingGameIds.Contains(game.Id.ToString())) continue;

                    // Convert genres
                    var genreNames = game.Genres.Values?
                        .Select(g => g.Name)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList();

                    // Extract publisher
                    var publisher = game.InvolvedCompanies?.Values?
                        .FirstOrDefault(ic => ic.Company != null && ic.Publisher == true)?.Company?.Value;


                    // Map to Media entity
                    var media = new Media {
                        Id = Guid.NewGuid(),
                        Title = game.Name,
                        PublishDate = game.FirstReleaseDate.HasValue
                            ? (game.FirstReleaseDate.Value).UtcDateTime
                            : DateTime.UtcNow,
                        Genre = string.Join(", ", genreNames),
                        CachedExternalId = game.Id.ToString(),
                        CachedImagePath = game.Cover?.Value?.ImageId != null
                            ? $"/{game.Cover.Value.ImageId}.jpg"
                            : null,
                        AvgRating = game.Rating ?? 0,
                        TotalReviews = (int)(game.RatingCount ?? 0),
                        Description = game.Summary,
                        Director = publisher?.Name ?? "Unknown",
                        Type = "game"
                    };

                    if (!string.IsNullOrEmpty(media.CachedImagePath))
                        mediaList.Add(media);
                }

                // Save to the database
                await _context.Media.AddRangeAsync(mediaList);
                await _context.SaveChangesAsync();

                return mediaList.Count;
            } catch (Exception ex) {
                Console.WriteLine($"Error fetching popular games: {ex.Message}");
                return 0;
            }
        }



    }

}
