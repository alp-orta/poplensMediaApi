using poplensMediaApi.Models;

namespace poplensMediaApi.Contracts {
    public interface IGameService {
        Task<int> FetchGamesAsync(GameFetchRequest request);
    }
}
