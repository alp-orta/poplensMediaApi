using poplensMediaApi.Models;

namespace poplensMediaApi.Contracts {

    public interface IMediaService {
        Task<Media> CreateMedia(Media media);
        Task<IEnumerable<Media>> GetAllMedia();
        Task<Media?> GetMediaById(Guid id);
        Task<bool> UpdateMedia(Guid id, Media updatedMedia);
        Task<bool> DeleteMedia(Guid id);
        Task<IEnumerable<Media>> SearchMedia(string query);
        Task<IEnumerable<Media>> SearchFilms(string query);
        Task<IEnumerable<Media>> SearchBooks(string query);
        Task<IEnumerable<Media>> SearchGames(string query);
    }

}
