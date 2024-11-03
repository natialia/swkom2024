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
        // Create a mapping configuration and add the mapping profile
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile()); // Assuming MappingProfile contains the mappings
        });

        _mapper = mappingConfig.CreateMapper(); // Create the mapper instance
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Map_Correctly()
    {
        var documentItem = new DocumentItem
        {
            Id = 1, // Set Id property
            Name = "Sample Item" // Set Name property
        };

        var documentDto = _mapper.Map<DocumentDTO>(documentItem); // Perform the mapping

        Assert.NotNull(documentDto); // Assert that the DTO is not null
        Assert.Equal(documentItem.Id, documentDto.Id); // Assert that the Id is mapped correctly
        Assert.Equal("*Sample Item*", documentDto.Name); // Assert that the Name is mapped with asterisks
    }

    [Fact]
    public void DocumentDTO_To_DocumentItem_Should_Map_Correctly()
    {
        var documentDto = new DocumentDTO
        {
            Id = 2, // Set Id property
            Name = "*Another Sample Item*" // Set Name property with asterisks
        };

        var documentItem = _mapper.Map<DocumentItem>(documentDto); // Perform the mapping

        Assert.NotNull(documentItem); // Assert that the DocumentItem is not null
        Assert.Equal(documentDto.Id, documentItem.Id); // Assert that the Id is mapped correctly
        Assert.Equal("Another Sample Item", documentItem.Name); // Assert that the Name is mapped without asterisks
    }
}
