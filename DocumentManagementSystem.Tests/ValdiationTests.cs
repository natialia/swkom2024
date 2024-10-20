using FluentValidation.TestHelper;
using DocumentManagementSystem.DTOs;
using Xunit;

public class DocumentDTOValidatorTests
{
    private readonly DocumentDTOValidator _validator;

    public DocumentDTOValidatorTests()
    {
        _validator = new DocumentDTOValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var documentDto = new DocumentDTO { Name = "" };

        // Act
        var result = _validator.TestValidate(documentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange
        var documentDto = new DocumentDTO { Name = null };

        // Act
        var result = _validator.TestValidate(documentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
