using dms_bl.Models;
using Elastic.Clients.Elasticsearch;

namespace DocumentManagementSystem.Controllers
{
    public interface IElasticSearchClientAgent
    {
        Task<MyIndexResponse> IndexAsync(Document document, string v);
        Task EnsureIndexExists();
        Task<SearchResponse<T>> SearchAsync<T>(Action<SearchRequestDescriptor<T>> request);
    }

    public class ElasticSearchClientAgent: IElasticSearchClientAgent
    {
        private readonly ElasticsearchClient _elasticClient;
        private readonly ILogger<ElasticSearchClientAgent> _logger;

        public ElasticSearchClientAgent(IConfiguration config, ILogger<ElasticSearchClientAgent> logger)
        {
            var elasticUri = config.GetConnectionString("ElasticSearch") ?? "http://elasticsearch:9200";
            var settings = new ElasticsearchClientSettings(new Uri(elasticUri)).EnableDebugMode();
            _elasticClient = new ElasticsearchClient(settings);
            _logger = logger;
        }
        public async Task EnsureIndexExists()
        {
            _elasticClient.SearchAsync<Document>();
            var indexName = "documents";

            //Check if index exists
            _logger.LogInformation("Checking if index exists...");
            var indexExistsResponse = await _elasticClient.Indices.ExistsAsync(indexName);

            if (!indexExistsResponse.Exists)
            {
                // if index doesnt exist: create
                _logger.LogWarning($"Creating new index {indexName}");
                await _elasticClient.Indices.CreateAsync(indexName);
            }
        }

        public async Task<MyIndexResponse> IndexAsync(Document document, string indexName)
        {
            var response = await _elasticClient.IndexAsync(document, i => i.Index(indexName));
            MyIndexResponse indexResponse = new MyIndexResponse()
            {
                IsValidResponse = response.IsValidResponse,
                DebugInformation = response.DebugInformation
            };
            return indexResponse;
        }

        public async Task<SearchResponse<T>> SearchAsync<T>(Action<SearchRequestDescriptor<T>> request)
        {
            return await _elasticClient.SearchAsync(request);
        }
    }

    public class DummyElasticSearchClient : IElasticSearchClientAgent
    {
        
        public Task EnsureIndexExists()
        {
            return Task.CompletedTask;
        }

        public Task<MyIndexResponse> IndexAsync(Document document, string v)
        {
            return Task.FromResult(new MyIndexResponse() { IsValidResponse = true, 
                DebugInformation = $"{v} was indexed successfully" });
        }

        public Task<SearchResponse<T>> SearchAsync<T>(Action<SearchRequestDescriptor<T>> request)
        {
            return Task.FromResult(new SearchResponse<T>());
        }
    }
    public class MyIndexResponse
    {
        public bool IsValidResponse { get; set; }
        public string DebugInformation { get; set; }
    }
}