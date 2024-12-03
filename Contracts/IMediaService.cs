using poplensMediaApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace poplensMediaApi.Contracts {

    public interface IMediaService {
        Task<Media> CreateMedia(Media media);
        Task<IEnumerable<Media>> GetAllMedia();
        Task<Media?> GetMediaById(Guid id);
        Task<bool> UpdateMedia(Guid id, Media updatedMedia);
        Task<bool> DeleteMedia(Guid id);
        Task<IEnumerable<Media>> SearchMedia(string query);
    }

}
