using Pgvector;

namespace poplensMediaApi.Services {
    public interface IEmbeddingProxyService {
        Task<Vector?> GetEmbeddingAsync(string reviewContent);
    }

    public class EmbeddingProxyService : IEmbeddingProxyService {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _embeddingApiUrl = "http://python-embedding-api:8000/generate-embedding/";


        public EmbeddingProxyService(IHttpClientFactory httpClientFactory) {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClient() {
            return _httpClientFactory.CreateClient();
        }

        public async Task<Vector?> GetEmbeddingAsync(string reviewContent) {
            var client = CreateHttpClient();
            var payload = new { text = reviewContent };
            var response = await client.PostAsJsonAsync(_embeddingApiUrl, payload);

            if (!response.IsSuccessStatusCode) {
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
            if (result?.Embedding == null) {
                return null;
            }

            // Convert List<float> to ReadOnlyMemory<float>
            var embeddingArray = result.Embedding.ToArray();
            return new Vector(new ReadOnlyMemory<float>(embeddingArray));
        }

        private class EmbeddingResponse {
            public List<float> Embedding { get; set; }
        }
    }
}
