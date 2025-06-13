using Pgvector;
using poplensMediaApi.Models;

namespace poplensMediaApi.Contracts {

    public interface IMediaService {
        Task<Media> CreateMedia(Media media);
        Task<Media?> GetMediaById(Guid id);
        Task<Media?> GetMediaWithEmbeddingById(Guid id);
        Task<bool> UpdateMedia(Guid id, Media updatedMedia);
        Task<bool> IncrementTotalReviewCount(Guid id);
        Task<bool> DeleteMedia(Guid id);
        Task<IEnumerable<Media>> SearchMedia(string query);
        Task<IEnumerable<Media>> SearchFilms(string query);
        Task<IEnumerable<Media>> SearchBooks(string query);
        Task<IEnumerable<Media>> SearchGames(string query);
        Task<IEnumerable<Media>> GetMediaWithFilters(string mediaType, string? decade, string? genre, string? sortBy, string? query, int page, int pageSize);
        Task<int> GetTotalMediaCount(string mediaType, string? decade, string? genre, string? query);
        Task<int> UpdateMissingMediaEmbeddingsAsync();
        Task<List<Media>> GetSimilarMediaAsync(Vector embedding, int count, string? mediaType = null, List<Guid>? excludedMediaIds = null);

    }

}
