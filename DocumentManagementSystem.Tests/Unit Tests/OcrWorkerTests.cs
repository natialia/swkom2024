using Xunit;
using System.IO;
using Moq;
using RabbitMQ.Client;
using System;
using ocr_worker;
using RabbitMQ.Client.Events;
using Minio.DataModel.Args;
using Minio;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

public class OcrWorkerTests
{
    [Fact]
    public void PerformOcr_ShouldExtractTextFromTestPngFile()
    {
        // Arrange: Mock RabbitMQ components and provide a valid test PNG file
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        mockConnection.Setup(c => c.CreateModel()).Returns(mockChannel.Object);

        string filePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Test.png");

        // Verify the test file exists
        Assert.True(File.Exists(filePath), $"Test file is missing: {filePath}");

        using var fileStream = new FileStream(  filePath, FileMode.Open, FileAccess.Read);
        Assert.True(fileStream.Length > 0, "Filestream could not read file");
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act: Perform OCR on the test file
        var extractedText = ocrWorker.PerformOcr(fileStream);

        // Assert: Ensure extracted text matches expectations
        Assert.False(string.IsNullOrEmpty(extractedText), "Extracted text is null or empty.");
        Assert.Contains("Test test", extractedText.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PerformOcr_Should_ReturnEmptyText_When_FileStream_Is_Empty()
    {
        // Arrange: Empty file stream setup
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        using var emptyStream = new MemoryStream();
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act: Perform OCR on an empty stream
        var result = ocrWorker.PerformOcr(emptyStream);

        // Assert: Verify that no text is extracted
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void PerformOcr_Should_Handle_InvalidFileFormat()
    {
        // Arrange: Set up a mock stream with invalid data
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        using var invalidStream = new MemoryStream(new byte[] { 0x00, 0x01, 0x02 });
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act: Perform OCR on invalid data
        var result = ocrWorker.PerformOcr(invalidStream);

        // Assert: Verify that the result is empty
        Assert.NotNull(result);
        Assert.True(string.IsNullOrEmpty(result));
    }

    [Fact]
    public void PerformOcr_Should_Handle_OCR_Exception()
    {
        // Arrange: Mock dependencies and simulate an exception
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        var fileStreamMock = new Mock<Stream>();
        fileStreamMock.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                      .Throws(new IOException("Simulated read error"));

        // Act: Handle the exception during OCR
        var result = ocrWorker.PerformOcr(fileStreamMock.Object);

        // Assert: Verify that result is empty after the exception
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void PerformOcr_Should_Handle_Empty_File_Stream()
    {
        // Arrange: Mock RabbitMQ components and create empty stream
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();

        using var emptyStream = new MemoryStream();
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act: Perform OCR on empty stream
        var result = ocrWorker.PerformOcr(emptyStream);

        // Assert: Ensure empty result
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void PerformOcr_Should_Throw_Exception_On_Invalid_Stream_Read()
    {
        // Arrange: Set up stream that throws an exception when read
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();

        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                  .Throws(new IOException("Simulated read error"));

        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act & Assert: Ensure exception handling doesn't crash OCR
        var result = ocrWorker.PerformOcr(mockStream.Object);
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);
    }
    
    [Fact]
    public void Dispose_Should_Close_Connection_And_Channel()
    {
        // Arrange: Mock RabbitMQ components
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act: Dispose the worker
        ocrWorker.Dispose();

        // Assert: Ensure that both connection and channel are closed
        mockConnection.Verify(c => c.Close(), Times.Once);
        mockChannel.Verify(c => c.Close(), Times.Once);
    }

    [Fact]
    public void PerformOcr_Should_Handle_Empty_Input_Stream()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();
        using var emptyStream = new MemoryStream();  // Empty file

        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act
        var result = ocrWorker.PerformOcr(emptyStream);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result);  // Should return an empty string
    }

    [Fact]
    public void PerformOcr_Should_Return_Text_For_Simulated_Image()
    {
        // Arrange
        var mockConnection = new Mock<IConnection>();
        var mockChannel = new Mock<IModel>();

        // Create a simulated image with text for OCR
        using var image = new Bitmap(200, 100);
        using var graphics = Graphics.FromImage(image);
        graphics.Clear(Color.White);
        graphics.DrawString("Sample Text", new Font("Arial", 20), Brushes.Black, new PointF(10, 40));

        // Save the image to a memory stream
        using var memoryStream = new MemoryStream();
        image.Save(memoryStream, ImageFormat.Png);
        memoryStream.Seek(0, SeekOrigin.Begin);  // Reset stream position

        var ocrWorker = new OcrWorker(mockConnection.Object, mockChannel.Object);

        // Act
        var result = ocrWorker.PerformOcr(memoryStream);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Sample Text", result, StringComparison.OrdinalIgnoreCase);
    }
}
