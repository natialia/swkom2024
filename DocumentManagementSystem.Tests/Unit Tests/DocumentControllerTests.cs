using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using DocumentManagementSystem.Controllers;
using dms_bl.Services;
using dms_bl.Models;
using dms_api.DTOs;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DocumentManagementSystem.DTOs;
using dms_dal_new.Entities;
using Microsoft.AspNetCore.Http;
using System.Text;
using Elastic.Clients.Elasticsearch;

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
        _mockDocumentService.Setup(service => service.GetAllDocumentsAsync()).ThrowsAsync(new System.Exception("Some error"));

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

    [Fact]
    public async Task PutDocument_ShouldReturnNoContent_WhenDocumentIsUpdated()
    {
        // Arrange
        var documentDto = new DocumentDTO { Id = 1, Name = "Updated Document" };
        var document = new Document { Id = 1, Name = "Updated Document" };
        var serviceResponse = new ServiceResponse { Success = true, Message = "Successfully updated document" };

        _mockMapper.Setup(m => m.Map<Document>(documentDto)).Returns(document);
        _mockDocumentService.Setup(service => service.UpdateDocumentAsync(1, document)).ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.PutDocument(1, documentDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNotFound_WhenDocumentDoesNotExist()
    {
        // Arrange
        _mockDocumentService.Setup(service => service.DeleteAsync(1)).ReturnsAsync(new ServiceResponse { Success = false, Message = "Could not delete: does not exist" });

        // Act
        var result = await _controller.DeleteDocument(1);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task SearchByQueryString_ShouldReturnBadRequest_WhenSearchTermIsEmpty()
    {
        // Act
        var result = await _controller.SearchByQueryString("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PutDocument_ShouldReturnBadRequest_WhenIdsDoNotMatch()
    {
        // Arrange
        var documentDto = new DocumentDTO { Id = 2, Name = "Updated Document" };

        // Act
        var result = await _controller.PutDocument(1, documentDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ID mismatch", badRequestResult.Value);
    }

}
