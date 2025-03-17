namespace poplensMediaApi.Contracts {
    public interface IBookService {
        Task<int> FetchPopularBooksAsync(string subject, int limit = 40, int offset = 0);
    }
}
