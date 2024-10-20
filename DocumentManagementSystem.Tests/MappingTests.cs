using AutoMapper;
using dms_dal.Entities;
using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Mappings;
using Xunit;

public class MappingTests
{
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MappingProfile());
        });

        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void DocumentItem_To_DocumentDTO_Should_Map_Correctly()
    {
        // Arrange
        var documentItem = new DocumentItem
        {
            Id = 1,
            Name = "Sample Item",
            IsComplete = true
        };

        // Act
        var documentDto = _mapper.Map<DocumentDTO>(documentItem);

        // Assert
        Assert.NotNull(documentDto);
        Assert.Equal(documentItem.Id, documentDto.Id);
        Assert.Equal("*Sample Item*", documentDto.Name); // Check for asterisks
        Assert.Equal(documentItem.IsComplete, documentDto.IsComplete);
    }

    [Fact]
    public void DocumentDTO_To_DocumentItem_Should_Map_Correctly()
    {
        // Arrange
        var documentDto = new DocumentDTO
        {
            Id = 2,
            Name = "*Another Sample Item*",
            IsComplete = false
        };

        // Act
        var documentItem = _mapper.Map<DocumentItem>(documentDto);

        // Assert
        Assert.NotNull(documentItem);
        Assert.Equal(documentDto.Id, documentItem.Id);
        Assert.Equal("Another Sample Item", documentItem.Name); // Check for removed asterisks
        Assert.Equal(documentDto.IsComplete, documentItem.IsComplete);
    }
}
