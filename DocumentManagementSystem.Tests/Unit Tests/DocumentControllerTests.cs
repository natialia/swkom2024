using AutoMapper;
using dms_bl.Services;
using DocumentManagementSystem.Controllers;
using DocumentManagementSystem.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using dms_bl.Models;
using dms_api.DTOs;
using Microsoft.AspNetCore.Http;
using Azure;
public class DocumentControllerTests
{
    private readonly Mock<IDocumentLogic> _mockDocumentService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<DocumentController>> _mockLogger;
    private readonly DocumentController _controller;

    public DocumentControllerTests()
    {
        _mockDocumentService = new Mock<IDocumentLogic>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<DocumentController>>();
        _controller = new DocumentController(_mockMapper.Object, _mockLogger.Object, _mockDocumentService.Object, null, null, null);
    }

    [Fact]
    public async Task GetAllDocuments_ShouldReturnOk_WhenDocumentsExist()
    {
        // Arrange
        var documents = new List<Document> { new Document { Id = 1, Name = "Doc1" }, new Document { Id = 2, Name = "Doc2" } };
        _mockDocumentService.Setup(service => service.GetAllDocumentsAsync()).ReturnsAsync(documents);
        var documentDtos = new List<DocumentDTO> { new DocumentDTO { Id = 1, Name = "Doc1" }, new DocumentDTO { Id = 2, Name = "Doc2" } };
        _mockMapper.Setup(m => m.Map<IEnumerable<DocumentDTO>>(documents)).Returns(documentDtos);

        // Act
        var result = await _controller.GetAllDocuments();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<List<DocumentDTO>>(okResult.Value);
        Assert.Equal(2, returnValue.Count);
    }

    [Fact]
    public async Task GetAllDocuments_ShouldReturnServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockDocumentService.Setup(service => service.GetAllDocumentsAsync()).ThrowsAsync(new Exception("Some error"));

        // Act
        var result = await _controller.GetAllDocuments();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task GetDocument_ShouldReturnDocument_WhenDocumentExists()
    {
        // Arrange
        var document = new Document { Id = 1, Name = "Test Document" };
        var documentDto = new DocumentDTO { Id = 1, Name = "Test Document" };
        _mockDocumentService.Setup(service => service.GetDocumentByIdAsync(1)).ReturnsAsync(document);
        _mockMapper.Setup(m => m.Map<DocumentDTO>(document)).Returns(documentDto);

        // Act
        var result = await _controller.GetDocument(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<DocumentDTO>(okResult.Value);
        Assert.Equal("Test Document", returnValue.Name);
    }

    [Fact]
    public async Task GetDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        _mockDocumentService.Setup(service => service.GetDocumentByIdAsync(1)).ReturnsAsync((Document)null);

        // Act
        var result = await _controller.GetDocument(1);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task PostDocument_ShouldReturnBadRequest_WhenNoFileIsUploaded()
    {
        // Arrange
        var documentRequest = new DocumentRequest
        {
            Id = 1,
            Name = "Test Document",
            FileType = "pdf",
            FileSize = "500KB",
            UploadedDocument = null
        };

        // Act
        var result = await _controller.PostDocument(documentRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
