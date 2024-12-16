using AutoMapper;
using dms_dal_new.Entities;
using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Mappings;
using Xunit;

public class MappingTests
{
    private readonly IMapper _mapper; // Mapper instance for testing

    // Constructor to set up the AutoMapper configuration for the tests
    public MappingTests()
    {
        // Arrange: Create a mapping configuration and add the mapping profile
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile()); // Assuming MappingProfile contains the mappings
        });

        _mapper = mappingConfig.CreateMapper(); // Create the mapper instance
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Map_Correctly()
    {
        // Arrange: Prepare the source DocumentItem entity
        var documentItem = new DocumentItem
        {
            Id = 1, // Set Id property
            Name = "Sample Item" // Set Name property
        };

        // Act: Map DocumentItem to DocumentDTO
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert: Verify that the properties are correctly mapped
        Assert.NotNull(documentDto); // DTO should not be null
        Assert.Equal(documentItem.Id, documentDto.Id); // Id should be mapped correctly
        Assert.Equal("*Sample Item*", documentDto.Name); // Name should be mapped with asterisks
    }

    [Fact]
    public void DocumentDTO_To_DocumentItem_Should_Map_Correctly()
    {
        // Arrange: Prepare the source DocumentDTO entity
        var documentDto = new DocumentDTO
        {
            Id = 2, // Set Id property
            Name = "*Another Sample Item*" // Set Name property with asterisks
        };

        // Act: Map DocumentDTO to DocumentItem
        var documentItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert: Verify that the properties are correctly mapped
        Assert.NotNull(documentItem); // DocumentItem should not be null
        Assert.Equal(documentDto.Id, documentItem.Id); // Id should be mapped correctly
        Assert.Equal("Another Sample Item", documentItem.Name); // Name should have asterisks removed
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Handle_Null_Source()
    {
        // Arrange: Null source object
        DocumentItem documentItem = null;

        // Act: Map the null object
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert: Result should be null
        Assert.Null(documentDto);
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_And_Back_Should_Preserve_Data()
    {
        // Arrange: Original DocumentItem
        var originalItem = new DocumentItem
        {
            Id = 4,
            Name = "RoundTrip Test"
        };

        // Act: Map to DTO and back to Entity
        var documentDto = _mapper.Map<DocumentDTO>(originalItem);
        var mappedBackItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert: Verify data integrity
        Assert.Equal(originalItem.Id, mappedBackItem.Id);
        Assert.Equal(originalItem.Name, mappedBackItem.Name);
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Handle_Large_Name()
    {
        // Arrange: Large string for Name
        var largeName = new string('A', 10000);
        var documentItem = new DocumentItem
        {
            Id = 5,
            Name = largeName
        };

        // Act: Map DocumentItem to DocumentDTO
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert: Name should map completely
        Assert.NotNull(documentDto);
        Assert.Equal($"*{largeName}*", documentDto.Name);
    }
    [Fact]
    public void DocumentDTO_To_DocumentItem_Should_Handle_Null_Source()
    {
        // Arrange: Null source object
        DocumentDTO documentDto = null;

        // Act: Map the null object
        var documentItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert: Result should be null
        Assert.Null(documentItem);
    }

    [Fact]
    public void DocumentDTO_To_DocumentItem_Should_Ignore_Unmapped_Properties()
    {
        // Arrange: Extra properties that should be ignored
        var documentDto = new DocumentDTO
        {
            Id = 6,
            Name = "Ignore Unmapped Test",
            FileType = "PDF",
            FileSize = "2048",
            OcrText = "Sample OCR Text"
        };

        // Act: Map to DocumentItem
        var documentItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert: Ensure only mapped properties are transferred
        Assert.NotNull(documentItem);
        Assert.Equal(documentDto.Id, documentItem.Id);
        Assert.Equal("Ignore Unmapped Test", documentItem.Name);
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Handle_Special_Characters()
    {
        // Arrange: DocumentItem with special characters
        var specialChars = "!@#$%^&*()_+<>?";
        var documentItem = new DocumentItem
        {
            Id = 7,
            Name = $"Special {specialChars} Test"
        };

        // Act: Map DocumentItem to DocumentDTO
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert: Ensure special characters are preserved
        Assert.NotNull(documentDto);
        Assert.Equal($"*Special {specialChars} Test*", documentDto.Name);
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Handle_Empty_Strings()
    {
        // Arrange: DocumentItem with empty Name
        var documentItem = new DocumentItem
        {
            Id = 8,
            Name = string.Empty
        };

        // Act: Map to DocumentDTO
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert: Ensure Name is still mapped correctly
        Assert.NotNull(documentDto);
        Assert.Equal("**", documentDto.Name);
    }

    [Fact]
    public void DocumentDTO_To_DocumentItem_Should_Handle_Numeric_Values_In_Name()
    {
        // Arrange: DocumentDTO with numeric Name
        var documentDto = new DocumentDTO
        {
            Id = 9,
            Name = "*1234567890*"
        };

        // Act: Map DocumentDTO to DocumentItem
        var documentItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert: Ensure numeric characters are mapped correctly
        Assert.NotNull(documentItem);
        Assert.Equal("1234567890", documentItem.Name);
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Preserve_Whitespace()
    {
        // Arrange: DocumentItem with leading/trailing whitespace
        var documentItem = new DocumentItem
        {
            Id = 10,
            Name = "   Document With Whitespace   "
        };

        // Act: Map DocumentItem to DocumentDTO
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert: Ensure whitespace is preserved
        Assert.NotNull(documentDto);
        Assert.Equal("*   Document With Whitespace   *", documentDto.Name);
    }
}
