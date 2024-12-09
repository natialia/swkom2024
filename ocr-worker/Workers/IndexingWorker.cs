using Microsoft.Extensions.Hosting;
using ocr_worker.Services;

namespace ocr_worker.Workers
{
    public class IndexingWorker : BackgroundService
    {
        private readonly IElasticSearchService _elasticSearchService;

        public IndexingWorker(IElasticSearchService elasticSearchService)
        {
            _elasticSearchService = elasticSearchService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Simulate OCR result indexing
                var id = Guid.NewGuid().ToString();
                var content = "Sample OCR text content";

                await _elasticSearchService.StoreOCRResultAsync(id, content);

                await Task.Delay(5000, stoppingToken); // Simulated delay
            }
        }
    }
}
