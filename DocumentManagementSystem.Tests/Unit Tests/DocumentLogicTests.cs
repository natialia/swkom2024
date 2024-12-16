using Moq;
using Xunit;
using dms_bl.Services;
using dms_bl.Models;
using dms_dal_new.Entities;
using dms_dal_new.Repositories;
using dms_bl.Exceptions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;

public class DocumentLogicTests
{
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IValidator<Document>> _mockValidator;
    private readonly DocumentLogic _documentLogic;

    public DocumentLogicTests()
    {
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockValidator = new Mock<IValidator<Document>>();

        _documentLogic = new DocumentLogic(_mockDocumentRepository.Object, _mockMapper.Object, _mockValidator.Object);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnMappedDocument_WhenFound()
    {
        // Arrange
        var documentId = 1;
        var documentItem = new DocumentItem { Id = documentId, Name = "Test Document" };
        var document = new Document { Id = documentId, Name = "Test Document" };

        _mockDocumentRepository.Setup(repo => repo.GetByIdAsync(documentId)).ReturnsAsync(documentItem);
        _mockMapper.Setup(m => m.Map<Document>(documentItem)).Returns(document);

        // Act
        var result = await _documentLogic.GetDocumentByIdAsync(documentId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(documentId, result.Id);
        _mockMapper.Verify(m => m.Map<Document>(documentItem), Times.Once);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var documentId = 1;
        _mockDocumentRepository.Setup(repo => repo.GetByIdAsync(documentId)).ReturnsAsync((DocumentItem)null);

        // Act
        var result = await _documentLogic.GetDocumentByIdAsync(documentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDocumentByIdAsync_ShouldThrowBusinessException_WhenErrorOccurs()
    {
        // Arrange
        var documentId = 1;
        _mockDocumentRepository.Setup(repo => repo.GetByIdAsync(documentId)).Throws(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessException>(() => _documentLogic.GetDocumentByIdAsync(documentId));
        Assert.Contains("Error getting document by ID", exception.Message);
    }

    [Fact]
    public async Task GetAllDocumentsAsync_ShouldReturnMappedDocuments()
    {
        // Arrange
        var documentItems = new List<DocumentItem> { new DocumentItem { Id = 1, Name = "Document 1" } };
        var documents = new List<Document> { new Document { Id = 1, Name = "Document 1" } };

        _mockDocumentRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(documentItems);
        _mockMapper.Setup(m => m.Map<IEnumerable<Document>>(documentItems)).Returns(documents);

        // Act
        var result = await _documentLogic.GetAllDocumentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Document 1", result.First().Name);
    }

    [Fact]
    public async Task AddDocumentAsync_ShouldReturnNull_WhenValidationFails()
    {
        // Arrange
        var document = new Document { Id = 1, Name = "Invalid Document" };
        var validationResult = new FluentValidation.Results.ValidationResult(new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Name", "Invalid name")
        });

        _mockValidator.Setup(v => v.Validate(document)).Returns(validationResult);

        // Act
        var result = await _documentLogic.AddDocumentAsync(document);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddDocumentAsync_ShouldReturnMappedDocumentItem_WhenValid()
    {
        // Arrange
        var document = new Document { Id = 1, Name = "Valid Document" };
        var documentItem = new DocumentItem { Id = 1, Name = "Valid Document" };
        _mockValidator.Setup(v => v.Validate(document)).Returns(new FluentValidation.Results.ValidationResult());
        _mockMapper.Setup(m => m.Map<DocumentItem>(document)).Returns(documentItem);
        _mockDocumentRepository.Setup(repo => repo.AddAsync(documentItem)).ReturnsAsync(documentItem);

        // Act
        var result = await _documentLogic.AddDocumentAsync(document);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(document.Name, result.Name);
    }

    


    [Fact]
    public async Task DeleteAsync_ShouldReturnFailure_WhenDocumentNotFound()
    {
        // Arrange
        var documentId = 1;
        _mockDocumentRepository.Setup(repo => repo.ContainsItem(documentId)).ReturnsAsync(false);

        // Act
        var result = await _documentLogic.DeleteAsync(documentId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Could not delete", result.Message);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenDocumentDeleted()
    {
        // Arrange
        var documentId = 1;
        _mockDocumentRepository.Setup(repo => repo.ContainsItem(documentId)).ReturnsAsync(true);
        _mockDocumentRepository.Setup(repo => repo.DeleteAsync(documentId)).Returns(Task.CompletedTask);

        // Act
        var result = await _documentLogic.DeleteAsync(documentId);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("Successfully deleted document", result.Message);
    }
}
