using dms_dal_new.Data;
using dms_dal_new.Entities;
using dms_dal_new.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class DocumentItemRepositoryTests
{
    private readonly DocumentRepository _repository; // Instance of the repository to be tested
    private readonly DocumentContext _context; // In-memory database context for testing

    // Constructor to set up the test environment
    public DocumentItemRepositoryTests()
    {
        // Configuring an in-memory database for isolated testing
        var options = new DbContextOptionsBuilder<DocumentContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid()) // Unique DB for each test run
            .Options;

        _context = new DocumentContext(options); // Create a new context using the in-memory options
        _repository = new DocumentRepository(_context); // Instantiate the repository with the context
    }

    [Fact]
    public async Task AddAsync_ShouldAddDocumentItem()
    {
        // Arrange
        var documentItem = new DocumentItem() // Creating a new DocumentItem using the parameterless constructor
        {
            Name = "Test Item" // Setting the Name property
        };

        // Act
        await _repository.AddAsync(documentItem); // Call the AddAsync method to add the item

        // Assert
        var itemInDb = await _context.DocumentItems.FindAsync(documentItem.Id); // Retrieve the item from the in-memory database
        Assert.NotNull(itemInDb); // Assert that the item was added
        Assert.Equal("Test Item", itemInDb.Name); // Assert that the Name property is as expected
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnDocumentItem()
    {
        // Arrange
        var documentItem = new DocumentItem() // Creating a new DocumentItem
        {
            Name = "Test Item" // Setting the Name property
        };

        await _repository.AddAsync(documentItem); // Add the item to the database

        // Act
        var result = await _repository.GetByIdAsync(documentItem.Id); // Retrieve the item by its ID

        // Assert
        Assert.NotNull(result); // Assert that the item is not null
        Assert.Equal("Test Item", result.Name); // Assert that the Name property is correct
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenDocumentItemNotFound()
    {
        // Act
        var result = await _repository.GetByIdAsync(999); // Attempt to retrieve an item with a non-existent ID

        // Assert
        Assert.Null(result); // Assert that the result is null
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveDocumentItem()
    {
        // Arrange
        var documentItem = new DocumentItem { Name = "Item to Delete" }; // Creating a new DocumentItem
        await _repository.AddAsync(documentItem); // Add the item to the database

        // Act
        await _repository.DeleteAsync(documentItem.Id); // Call the DeleteAsync method to remove the item
        var itemInDb = await _context.DocumentItems.FindAsync(documentItem.Id); // Try to retrieve the deleted item

        // Assert
        Assert.Null(itemInDb); // Assert that the item was successfully removed
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateDocumentItem()
    {
        // Arrange
        var documentItem = new DocumentItem { Name = "Old Name" }; // Create a new DocumentItem
        await _repository.AddAsync(documentItem); // Add the item to the database

        documentItem.Name = "Updated Name"; // Update the Name property

        // Act
        await _repository.UpdateAsync(documentItem); // Call the UpdateAsync method to apply the change
        var updatedItem = await _context.DocumentItems.FindAsync(documentItem.Id); // Retrieve the updated item

        // Assert
        Assert.NotNull(updatedItem); // Assert that the updated item is not null
        Assert.Equal("Updated Name", updatedItem.Name); // Assert that the Name property has been updated
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllDocumentItems()
    {
        // Arrange: Add multiple items
        var documentItems = new List<DocumentItem>
    {
        new DocumentItem { Name = "Item 1" },
        new DocumentItem { Name = "Item 2" },
        new DocumentItem { Name = "Item 3" }
    };

        foreach (var item in documentItems)
        {
            await _repository.AddAsync(item);
        }

        // Act: Retrieve all items
        var allItems = await _repository.GetAllAsync();

        // Assert: Check the count and correct retrieval
        Assert.Contains(allItems, d => d.Name == "Item 1");
        Assert.Contains(allItems, d => d.Name == "Item 2");
        Assert.Contains(allItems, d => d.Name == "Item 3");
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenDocumentItemDoesNotExist()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var exception = await Record.ExceptionAsync(() => _repository.DeleteAsync(nonExistentId));

        // Assert
        Assert.Null(exception); // Verify that no exception was thrown
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoItemsExist()
    {
        // Act
        var allItems = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(allItems); // Ensure the result is not null
        Assert.Empty(allItems);   // Verify that the result is empty
    }

}
