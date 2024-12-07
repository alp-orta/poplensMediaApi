using poplensMediaApi.Models;

namespace poplensMediaApi.Contracts {
    public interface IGameService {
        Task<int> FetchGamesAsync(GameFetchRequest request);

        Task<int> FetchPopularGamesAsync(int limit = 100, int offset = 0);
    }
}
