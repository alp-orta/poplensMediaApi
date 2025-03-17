using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using poplensMediaApi.Contracts;
using poplensMediaApi.Data;
using poplensMediaApi.Models;
using poplensMediaApi.Models.Book;

namespace poplensMediaApi.Services {
    public class BookService : IBookService {
        private readonly MediaDbContext _context;
        private readonly HttpClient _httpClient;
        private const string GoogleBooksApiKey = "AIzaSyAEE2onbsmYuHexrJkfeobkXJm6EzlWPFU"; // Replace with your API key

        public BookService(MediaDbContext context, HttpClient httpClient) {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<int> FetchPopularBooksAsync(string subject, int limit = 40, int offset = 0) {
            var books = new List<Media>();
            var existingBookIds = await _context.Media
                .Where(m => m.Type == "book")
                .Select(m => m.CachedExternalId)
                .ToListAsync();

            while (books.Count < limit) {
                var query = $"https://www.googleapis.com/books/v1/volumes?q=subject:{subject}" +
                            $"&orderBy=relevance&maxResults=40&startIndex={offset}&key={GoogleBooksApiKey}";

                var response = await _httpClient.GetStringAsync(query);
                var googleBooksResponse = JsonConvert.DeserializeObject<GoogleBooksResponse>(response);

                if (googleBooksResponse?.Items == null || googleBooksResponse.Items.Count == 0)
                    break;

                foreach (var item in googleBooksResponse.Items) {
                    if (existingBookIds.Contains(item.Id)) continue;

                    var volumeInfo = item.VolumeInfo;
                    var authors = volumeInfo?.Authors ?? new List<string> { "Unknown" };
                    var genreNames = volumeInfo?.Categories ?? new List<string>();

                    // Convert publish date
                    DateTime publishDate = DateTime.UtcNow;
                    if (!string.IsNullOrWhiteSpace(volumeInfo?.PublishedDate) &&
                        DateTime.TryParse(volumeInfo?.PublishedDate, out var parsedDate)) {
                        publishDate = parsedDate.ToUniversalTime();
                    }

                    // Create Media object
                    var media = new Media {
                        Id = Guid.NewGuid(),
                        Title = volumeInfo.Title,
                        PublishDate = publishDate,
                        Genre = string.Join(", ", genreNames),
                        CachedExternalId = item.Id,
                        CachedImagePath = item.Id,
                        AvgRating = volumeInfo?.AverageRating ?? 0,
                        TotalReviews = volumeInfo?.RatingsCount ?? 0,
                        Description = volumeInfo?.Description ?? "Unknown",
                        Writer = string.Join(", ", authors),
                        Type = "book",
                        CreatedDate = DateTime.UtcNow,
                        LastUpdatedDate = DateTime.UtcNow
                    };

                    // Add only valid books
                    if (!existingBookIds.Contains(media.CachedExternalId) &&
                        media.CachedImagePath != null) {
                        books.Add(media);
                        existingBookIds.Add(media.CachedExternalId);
                    }

                    if (books.Count >= limit) break;
                }

                offset += 40;
            }

            // Sort by ratingsCount and averageRating
            books = books
                .OrderByDescending(b => b.TotalReviews)
                .ThenByDescending(b => b.AvgRating)
                .Take(limit)
                .ToList();

            await _context.Media.AddRangeAsync(books);
            await _context.SaveChangesAsync();

            return books.Count;
        }


    }
}
