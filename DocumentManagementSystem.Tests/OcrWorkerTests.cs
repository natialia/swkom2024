using Xunit;
using Moq;
using System.IO;
using ocr_worker;
using RabbitMQ.Client;
using ImageMagick;
using System;

public class OcrWorkerTests
{
    [Fact]
    public void PerformOcr_ShouldExtractTextFromSampleFile()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);

        // Ensure the sample file exists
        string sampleFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Test.png");
        Console.WriteLine($"Test file path: {sampleFilePath}");
        Assert.True(File.Exists(sampleFilePath), "Test file is missing. Ensure TestData/Test.png is copied to the output directory.");

        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act
        string extractedText = string.Empty;
        try
        {
            Console.WriteLine("Starting OCR processing...");
            extractedText = ocrWorker.PerformOcr(sampleFilePath);
            Console.WriteLine($"Extracted text: {extractedText}");
        }
        catch (Exception ex)
        {
            // Fail the test if an exception is thrown
            throw new InvalidOperationException($"PerformOcr threw an exception: {ex.Message}", ex);
        }

        // Assert
        Assert.False(string.IsNullOrEmpty(extractedText), "Extracted text is null or empty.");
        Assert.Contains("Test test", extractedText);

        // Clean up
        ocrWorker.Dispose();
    }
}
