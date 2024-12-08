using Xunit;
using System.IO;
using Moq;
using ocr_worker.Workers;
using RabbitMQ.Client;
using System;

public class OcrWorkerTests
{
    [Fact]
    public void PerformOcr_ShouldExtractTextFromTestPngFile()
    {
        // Arrange: Mock RabbitMQ components
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);

        string filePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Test.png");

        // Verify the test file exists
        Assert.True(File.Exists(filePath), $"Test file is missing: {filePath}");

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        // Inject mocks into the OcrWorker
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act
        var extractedText = ocrWorker.PerformOcr(fileStream);

        // Debugging Output
        Console.WriteLine($"Extracted OCR Text: {extractedText}");

        // Assert
        Assert.False(string.IsNullOrEmpty(extractedText), "Extracted text is null or empty.");
        Assert.Contains("Test test", extractedText.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
