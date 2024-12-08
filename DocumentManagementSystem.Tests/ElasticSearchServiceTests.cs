using Moq;
using Xunit;
using Nest;
using ocr_worker.Models;
using ocr_worker.Services;

public class ElasticsearchServiceTests
{
    [Fact]
    public async Task StoreOCRResultAsync_ShouldReturnTrue_WhenIndexingIsSuccessful()
    {
        // Arrange
        var mockClient = new Mock<IElasticClient>();

        // Create a mock response that simulates a valid index response
        var mockResponse = new Mock<IndexResponse>();
        mockResponse.SetupGet(x => x.IsValid).Returns(true);

        mockClient.Setup(x => x.IndexDocumentAsync(It.IsAny<OcrDocument>(), default))
                  .ReturnsAsync(mockResponse.Object);

        var service = new ElasticSearchService(mockClient.Object);

        // Act
        var result = await service.StoreOCRResultAsync("123", "Sample OCR Result");

        // Assert
        Assert.True(result);
    }
}
