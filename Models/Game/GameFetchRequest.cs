namespace poplensMediaApi.Models.Game
{
    public class GameFetchRequest
    {
        public int? ReleaseYear { get; set; }
        public string? Genre { get; set; }
        public string? Publisher { get; set; }
        public int PagesToFetch { get; set; } = 5; // Default to 5 pages
    }

}
