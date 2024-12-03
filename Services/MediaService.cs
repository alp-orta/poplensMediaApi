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

        public async Task<Media> CreateMedia(Media media) {
            media.Id = Guid.NewGuid();
            media.TotalReviews = 0; // Initialize to 0
            _context.Media.Add(media);
            await _context.SaveChangesAsync();
            return media;
        }

        public async Task<IEnumerable<Media>> GetAllMedia() {
            return await _context.Media.ToListAsync();
        }

        public async Task<Media?> GetMediaById(Guid id) {
            return await _context.Media.FindAsync(id);
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
            media.Type = updatedMedia.Type; // Update type
            media.Director = updatedMedia.Director;
            media.Writer = updatedMedia.Writer;
            media.Publisher = updatedMedia.Publisher;

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

        public async Task<IEnumerable<Media>> SearchMedia(string query) {
            return await _context.Media
                .Where(m => m.Title.Contains(query) || m.Description.Contains(query))
                .ToListAsync();
        }
    }

}
