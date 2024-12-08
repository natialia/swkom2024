public interface IElasticSearchService
{
    Task<bool> StoreOCRResultAsync(string id, string content);
}

