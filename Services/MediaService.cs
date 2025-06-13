using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using poplensMediaApi.Contracts;
using poplensMediaApi.Data;
using poplensMediaApi.Models;

namespace poplensMediaApi.Services {
    public class MediaService : IMediaService {
        private readonly MediaDbContext _context;
        private readonly IEmbeddingProxyService _embeddingProxyService;

        public MediaService(MediaDbContext context, IEmbeddingProxyService embeddingProxyService) {
            _context = context;
            _embeddingProxyService = embeddingProxyService;
        }

        public async Task<Media?> GetMediaById(Guid id) {
            // Exclude the Embedding property
            return await _context.Media
                .Where(m => m.Id == id)
                .Select(m => new Media {
                    Id = m.Id,
                    Title = m.Title,
                    PublishDate = m.PublishDate,
                    Genre = m.Genre,
                    CachedExternalId = m.CachedExternalId,
                    CachedImagePath = m.CachedImagePath,
                    AvgRating = m.AvgRating,
                    TotalReviews = m.TotalReviews,
                    Description = m.Description,
                    Type = m.Type,
                    Director = m.Director,
                    Writer = m.Writer,
                    Publisher = m.Publisher,
                    CreatedDate = m.CreatedDate,
                    LastUpdatedDate = m.LastUpdatedDate
                    // Embedding is intentionally excluded
                })
                .FirstOrDefaultAsync();
        }

        // New method to get media with embedding
        public async Task<Media?> GetMediaWithEmbeddingById(Guid id) {
            return await _context.Media
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Media> CreateMedia(Media media) {
            media.Id = Guid.NewGuid();
            media.AvgRating = 0; // Initialize to 0
            media.TotalReviews = 0; // Initialize to 0
            media.CreatedDate = DateTime.Now;
            media.LastUpdatedDate = DateTime.Now;
            _context.Media.Add(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task<bool> UpdateMedia(Guid id, Media updatedMedia) {
            var media = await _context.Media.FindAsync(id);
            if (media == null) return false;

            // Update fields
            media.Title = updatedMedia.Title;
            media.PublishDate = updatedMedia.PublishDate;
            media.Genre = updatedMedia.Genre;
            media.CachedExternalId = updatedMedia.CachedExternalId;
            media.CachedImagePath = updatedMedia.CachedImagePath;
            media.AvgRating = updatedMedia.AvgRating;
            media.TotalReviews = updatedMedia.TotalReviews;
            media.Description = updatedMedia.Description;
            media.Type = updatedMedia.Type;
            media.Director = updatedMedia.Director;
            media.Writer = updatedMedia.Writer;
            media.Publisher = updatedMedia.Publisher;
            media.LastUpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IncrementTotalReviewCount(Guid id) {
            var media = await _context.Media.FindAsync(id);
            if (media == null) return false;

            media.TotalReviews++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMedia(Guid id) {
            var media = await _context.Media.FindAsync(id);
            if (media == null) return false;

            _context.Media.Remove(media);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Query for media by title, director, writer, or publisher
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Media>> SearchMedia(string query) {
            string lowerQuery = query.ToLower();
            return await _context.Media
                .Where(m => m.Title.ToLower().Contains(lowerQuery)
                            || (m.Director != null && m.Director.ToLower().Contains(lowerQuery))
                            || (m.Writer != null && m.Writer.ToLower().Contains(lowerQuery))
                            || (m.Publisher != null && m.Publisher.ToLower().Contains(lowerQuery)))
                .Take(15)
                .ToListAsync();
        }

        /// <summary>
        /// Query for films by title or director
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Media>> SearchFilms(string query) {
            string lowerQuery = query.ToLower();
            return await _context.Media
                .Where(m => m.Type == "film" && (m.Title.ToLower().Contains(lowerQuery)
                            || (m.Director != null && m.Director.ToLower().Contains(lowerQuery))))
                .Take(15)
                .ToListAsync();
        }

        /// <summary>
        /// Query for books by title or writer
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Media>> SearchBooks(string query) {
            string lowerQuery = query.ToLower();
            return await _context.Media
                .Where(m => m.Type == "book" && (m.Title.ToLower().Contains(lowerQuery)
                            || (m.Writer != null && m.Writer.ToLower().Contains(lowerQuery))))
                .Take(15)
                .ToListAsync();
        }

        /// <summary>
        /// Query for games by title or publisher
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Media>> SearchGames(string query) {
            string lowerQuery = query.ToLower();
            return await _context.Media
                .Where(m => m.Type == "game" && (m.Title.ToLower().Contains(lowerQuery)
                            || (m.Publisher != null && m.Publisher.ToLower().Contains(lowerQuery))))
                .Take(15)
                .ToListAsync();
        }

        public async Task<IEnumerable<Media>> GetMediaWithFilters(string mediaType, string? decade, string? genre, string? sortBy, string? query, int page, int pageSize) {
            var mediaQuery = _context.Media.AsQueryable();

            // Filter by media type
            mediaQuery = mediaQuery.Where(m => m.Type == mediaType);

            // Filter by decade
            if (!string.IsNullOrEmpty(decade)) {
                int startYear = int.Parse(decade);
                int endYear = startYear + 9;
                mediaQuery = mediaQuery.Where(m => m.PublishDate.Year >= startYear && m.PublishDate.Year <= endYear);
            }

            // Filter by genre
            if (!string.IsNullOrEmpty(genre)) {
                mediaQuery = mediaQuery.Where(m => m.Genre.ToLower().Contains(genre.ToLower()));
            }

            // Filter by query
            if (!string.IsNullOrEmpty(query)) {
                string lowerQuery = query.ToLower();
                mediaQuery = mediaQuery.Where(m => m.Title.ToLower().Contains(lowerQuery)
                            || (m.Director != null && m.Director.ToLower().Contains(lowerQuery))
                            || (m.Writer != null && m.Writer.ToLower().Contains(lowerQuery))
                            || (m.Publisher != null && m.Publisher.ToLower().Contains(lowerQuery)));
            }

            // Sort by
            switch (sortBy) {
                case "rating-high":
                    mediaQuery = mediaQuery.OrderByDescending(m => m.AvgRating);
                    break;
                case "rating-low":
                    mediaQuery = mediaQuery.OrderBy(m => m.AvgRating);
                    break;
                case "comments-high":
                    mediaQuery = mediaQuery.OrderByDescending(m => m.TotalReviews);
                    break;
                case "comments-low":
                    mediaQuery = mediaQuery.OrderBy(m => m.TotalReviews);
                    break;
                default:
                    mediaQuery = mediaQuery.OrderBy(m => m.Title);
                    break;
            }

            // Pagination
            mediaQuery = mediaQuery.Skip((page - 1) * pageSize).Take(pageSize);

            return await mediaQuery.ToListAsync();
        }

        public async Task<int> GetTotalMediaCount(string mediaType, string? decade, string? genre, string? query) {
            var mediaQuery = _context.Media.AsQueryable();

            // Filter by media type
            mediaQuery = mediaQuery.Where(m => m.Type == mediaType);

            // Filter by decade
            if (!string.IsNullOrEmpty(decade)) {
                int startYear = int.Parse(decade);
                int endYear = startYear + 9;
                mediaQuery = mediaQuery.Where(m => m.PublishDate.Year >= startYear && m.PublishDate.Year <= endYear);
            }

            // Filter by genre
            if (!string.IsNullOrEmpty(genre)) {
                mediaQuery = mediaQuery.Where(m => m.Genre.ToLower().Contains(genre.ToLower()));
            }

            // Filter by query
            if (!string.IsNullOrEmpty(query)) {
                string lowerQuery = query.ToLower();
                mediaQuery = mediaQuery.Where(m => m.Title.ToLower().Contains(lowerQuery)
                            || (m.Director != null && m.Director.ToLower().Contains(lowerQuery))
                            || (m.Writer != null && m.Writer.ToLower().Contains(lowerQuery))
                            || (m.Publisher != null && m.Publisher.ToLower().Contains(lowerQuery)));
            }

            return await mediaQuery.CountAsync();
        }

        public async Task<List<Media>> GetSimilarMediaAsync(
    Vector embedding,
    int count,
    string? mediaType = null,
    List<Guid>? excludedMediaIds = null) {
            Console.WriteLine($"[GetSimilarMediaAsync] Called with count={count}, mediaType={mediaType}, excludedMediaIds.Count={excludedMediaIds?.Count ?? 0}");

            // Debug embedding
            var embeddingArray = embedding?.ToArray();
            Console.WriteLine($"[GetSimilarMediaAsync] Embedding is {(embeddingArray == null ? "null" : $"length {embeddingArray.Length}")}");
            if (embeddingArray != null && embeddingArray.Length > 0)
                Console.WriteLine($"[GetSimilarMediaAsync] Embedding sample: [{string.Join(", ", embeddingArray.Take(5))}...]");

            var query = _context.Media
                .Where(m => m.Embedding != null);

            Console.WriteLine("[GetSimilarMediaAsync] Initial query: Media with non-null embedding");

            if (!string.IsNullOrEmpty(mediaType)) {
                query = query.Where(m => m.Type == mediaType);
                Console.WriteLine($"[GetSimilarMediaAsync] Filtered by mediaType: {mediaType}");
            }

            if (excludedMediaIds != null && excludedMediaIds.Any()) {
                query = query.Where(m => !excludedMediaIds.Contains(m.Id));
                Console.WriteLine($"[GetSimilarMediaAsync] Excluding {excludedMediaIds.Count} media IDs");
            }

            var result = await query
                .OrderBy(m => m.Embedding.CosineDistance(embedding))
                .Take(count)
                .ToListAsync();

            Console.WriteLine($"[GetSimilarMediaAsync] Returning {result.Count} results");

            return result;
        }





        /// <summary>
        /// Updates all media records that are missing embeddings.
        /// </summary>
        /// <returns>The number of media records updated.</returns>
        public async Task<int> UpdateMissingMediaEmbeddingsAsync() {
            const int batchSize = 10000;
            int updatedCount = 0;

            while (true) {
                // Fetch the next batch of media without embeddings
                var batch = await _context.Media
                    .Where(m => m.Embedding == null)
                    .OrderBy(m => m.Id)
                    .Take(batchSize)
                    .ToListAsync();

                if (batch.Count == 0)
                    break;

                foreach (var media in batch) {
                    var embeddingInput = $"Type: {media.Type}; Title: {media.Title}; Genre: {media.Genre}; Description: {media.Description};";
                    var embedding = await _embeddingProxyService.GetEmbeddingAsync(embeddingInput);

                    if (embedding != null) {
                        media.Embedding = embedding;
                        media.LastUpdatedDate = DateTime.UtcNow;
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();
            }

            return updatedCount;
        }

    }
}
