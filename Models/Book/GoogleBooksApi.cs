namespace poplensMediaApi.Models.Book {
    public class GoogleBooksResponse {
        public List<GoogleBookItem> Items { get; set; }
    }

    public class GoogleBookItem {
        public string Id { get; set; }
        public GoogleVolumeInfo VolumeInfo { get; set; }
    }

    public class GoogleVolumeInfo {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string Publisher { get; set; }
        public string PublishedDate { get; set; }
        public double? AverageRating { get; set; }
        public int? RatingsCount { get; set; }
        public List<string> Categories { get; set; }
        public string Description { get; set; }
        public GoogleImageLinks ImageLinks { get; set; }
    }

    public class GoogleImageLinks {
        public string Thumbnail { get; set; }
    }
}
