using AutoMapper;
using DocumentManagementSystem.DTOs;
using dms_dal.Entities;
using Xunit;
using DocumentManagementSystem.Mappings;

public class MappingTests
{
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public void Should_Map_DocumentDTO_To_DocumentItem()
    {
        // Arrange
        var documentDto = new DocumentDTO
        {
            Id = 1, // Add the ID here
            Name = "Sample Name",
            IsComplete = false
        };

        // Act
        var documentItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert
        Assert.Equal(documentDto.Id, documentItem.Id);
        Assert.Equal("Sample Name", documentItem.Name); // No "*" since it's from DTO
        Assert.Equal(documentDto.IsComplete, documentItem.IsComplete);
    }

    [Fact]
    public void Should_Map_DocumentItem_To_DocumentDTO()
    {
        // Arrange
        var documentItem = new DocumentItem(1) // Provide ID here
        {
            Name = "Sample Name",
            IsComplete = false
        };

        // Act
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert
        Assert.Equal(documentItem.Id, documentDto.Id);
        Assert.Equal("*Sample Name*", documentDto.Name); // "*" should be added
        Assert.Equal(documentItem.IsComplete, documentDto.IsComplete);
    }
}
