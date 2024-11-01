using dms_dal_new.Data;
using dms_dal_new.Entities;
using dms_dal_new.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class DocumentItemRepositoryTests
{
    private readonly DocumentRepository _repository;
    private readonly DocumentContext _context;

    public DocumentItemRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DocumentContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid()) // Unique DB for each test run
            .Options;

        _context = new DocumentContext(options);
        _repository = new DocumentItemRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocumentItem()
    {
        // Arrange
        var documentItem = new DocumentItem() // Parameterless constructor
        {
            Name = "Test Item"
        };

        // Act
        await _repository.AddAsync(documentItem); // Call the method without assigning to a variable

        // Assert
        var itemInDb = await _context.DocumentItems.FindAsync(documentItem.Id); // Use the original documentItem to retrieve
        Assert.NotNull(itemInDb);
        Assert.Equal("Test Item", itemInDb.Name);
    }


    [Fact]
    public async Task GetByIdAsync_ShouldReturnDocumentItem()
    {
        // Arrange
        var documentItem = new DocumentItem() // Using parameterless constructor
        {
            Name = "Test Item"
        };

        await _repository.AddAsync(documentItem); // Directly call without assigning

        // Act
        var result = await _repository.GetByIdAsync(documentItem.Id); // Use documentItem.Id

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Item", result.Name);
    }
}
