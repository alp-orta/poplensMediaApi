namespace poplensMediaApi.Models {
    public class Media {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public string Genre { get; set; }
        public string CachedExternalId { get; set; }
        public string CachedImagePath { get; set; }
        public double AvgRating { get; set; }
        public int TotalReviews { get; set; } // Number of reviews
        public string Description { get; set; }
        public string Type { get; set; } // Determines if it's a film, book, or game

        // Specific to media types
        public string? Director { get; set; }  // For films
        public string? Writer { get; set; }   // For books
        public string? Publisher { get; set; } // For games
    }

}
