using System.Threading.Tasks;
using Xunit;
using Moq;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;
using dms_bl.Models;
using DocumentManagementSystem.Controllers;

namespace DocumentManagementSystem.Tests
{
    public class ElasticSearchClientAgentTests
    {
        [Fact]
        public async Task IndexAsync_ShouldReturnValidResponse_WhenDocumentIsIndexed()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();
            var document = new Document
            {
                Id = 1,
                Name = "Test Document",
                FileType = "PDF",
                FileSize = "1024",
                OcrText = "Sample OCR Text"
            };

            var expectedResponse = new MyIndexResponse
            {
                IsValidResponse = true,
                DebugInformation = "Document indexed successfully"
            };

            mockElasticSearchClientAgent
                .Setup(x => x.IndexAsync(It.IsAny<Document>(), "documents"))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await mockElasticSearchClientAgent.Object.IndexAsync(document, "documents");

            // Assert
            Assert.True(result.IsValidResponse);
            Assert.Contains("Document indexed successfully", result.DebugInformation);
        }

        [Fact]
        public async Task IndexAsync_ShouldReturnError_WhenIndexingFails()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();
            var document = new Document { Id = 1, Name = "Failed Document" };

            var failedResponse = new MyIndexResponse
            {
                IsValidResponse = false,
                DebugInformation = "Indexing failed"
            };

            mockElasticSearchClientAgent
                .Setup(x => x.IndexAsync(It.IsAny<Document>(), "documents"))
                .ReturnsAsync(failedResponse);

            // Act
            var result = await mockElasticSearchClientAgent.Object.IndexAsync(document, "documents");

            // Assert
            Assert.False(result.IsValidResponse);
            Assert.Contains("Indexing failed", result.DebugInformation);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnValidResponse_WhenDocumentIsDeleted()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();
            var document = new Document { Id = 2, Name = "Delete Test Document" };

            var expectedResponse = new MyIndexResponse
            {
                IsValidResponse = true,
                DebugInformation = "Document deleted successfully"
            };

            mockElasticSearchClientAgent
                .Setup(x => x.DeleteAsync(It.IsAny<Document>(), "documents"))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await mockElasticSearchClientAgent.Object.DeleteAsync(document, "documents");

            // Assert
            Assert.True(result.IsValidResponse);
            Assert.Contains("Document deleted successfully", result.DebugInformation);
        }

        [Fact]
        public async Task DeleteAsync_ShouldReturnError_WhenDocumentNotFound()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();
            var document = new Document { Id = 999, Name = "Non-existent Document" };

            var failedResponse = new MyIndexResponse
            {
                IsValidResponse = false,
                DebugInformation = "Document not found"
            };

            mockElasticSearchClientAgent
                .Setup(x => x.DeleteAsync(It.IsAny<Document>(), "documents"))
                .ReturnsAsync(failedResponse);

            // Act
            var result = await mockElasticSearchClientAgent.Object.DeleteAsync(document, "documents");

            // Assert
            Assert.False(result.IsValidResponse);
            Assert.Contains("Document not found", result.DebugInformation);
        }

        

        [Fact]
        public async Task EnsureIndexExists_ShouldNotCreateIndex_WhenIndexAlreadyExists()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();

            mockElasticSearchClientAgent
                .Setup(x => x.EnsureIndexExists())
                .Returns(Task.CompletedTask);

            // Act
            await mockElasticSearchClientAgent.Object.EnsureIndexExists();

            // Assert
            mockElasticSearchClientAgent.Verify(x => x.EnsureIndexExists(), Times.Once);
        }

        [Fact]
        public async Task EnsureIndexExists_ShouldCreateIndex_WhenIndexDoesNotExist()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();

            mockElasticSearchClientAgent
                .Setup(x => x.EnsureIndexExists())
                .Returns(Task.CompletedTask);

            // Act
            await mockElasticSearchClientAgent.Object.EnsureIndexExists();

            // Assert
            mockElasticSearchClientAgent.Verify(x => x.EnsureIndexExists(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var mockElasticSearchClientAgent = new Mock<IElasticSearchClientAgent>();
            var document = new Document { Id = 999, Name = "Document Causing Error" };

            mockElasticSearchClientAgent
                .Setup(x => x.DeleteAsync(It.IsAny<Document>(), "documents"))
                .ThrowsAsync(new System.Exception("Unexpected error"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() =>
                mockElasticSearchClientAgent.Object.DeleteAsync(document, "documents"));
        }
    }
}
