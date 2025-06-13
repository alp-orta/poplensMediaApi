namespace poplensMediaApi.Models {
    public class SimilarMediaRequest {
        public float[] Embedding { get; set; }
        public int Count { get; set; }
        public string? MediaType { get; set; }
        public List<Guid>? ExcludedMediaIds { get; set; }
    }
}
