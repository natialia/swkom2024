using Moq;
using ocr_worker.Services;
using ocr_worker.Workers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class IndexingWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldStoreOCRResults_WhenCalled()
    {
        // Arrange
        var mockElasticSearchService = new Mock<IElasticSearchService>();
        mockElasticSearchService.Setup(x => x.StoreOCRResultAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var indexingWorker = new IndexingWorker(mockElasticSearchService.Object);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(100); // Simulate quick cancellation for test

        // Act
        await indexingWorker.StartAsync(cancellationTokenSource.Token);

        // Assert
        mockElasticSearchService.Verify(x => x.StoreOCRResultAsync(It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }
}
