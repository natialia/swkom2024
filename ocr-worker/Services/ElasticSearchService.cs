using Elasticsearch.Net;
using ocr_worker.Models;
using Nest;

namespace ocr_worker.Services
{
    public class ElasticSearchService : IElasticSearchService
    {
        private readonly IElasticClient _elasticClient;

        public ElasticSearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<bool> StoreOCRResultAsync(string id, string content)
        {
            var result = await _elasticClient.IndexDocumentAsync(new OcrDocument
            {
                Id = id,
                OcrText = content
            });

            return result.IsValid;
        }
    }

}
