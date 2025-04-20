using Microsoft.EntityFrameworkCore;
using poplensMediaApi.Contracts;
using poplensMediaApi.Data;
using poplensMediaApi.Models;

namespace poplensMediaApi.Services {
    public class MediaService : IMediaService {
        private readonly MediaDbContext _context;

        public MediaService(MediaDbContext context) {
            _context = context;
        }
        public async Task<IEnumerable<Media>> GetAllMedia() {
            return await _context.Media.ToListAsync();
        }

        public async Task<Media?> GetMediaById(Guid id) {
            return await _context.Media.FindAsync(id);
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
    }

}
