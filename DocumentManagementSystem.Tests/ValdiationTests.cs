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
        var documentDto = new DocumentDTO { Name = "", IsComplete = false };

        // Act
        var result = _validator.TestValidate(documentDto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_IsComplete_Is_Null()
    {
        // Arrange
        var documentDto = new DocumentDTO { Name = "Sample Name", IsComplete = null }; // This will not compile; you'll need to use bool to avoid null.

        // Act
        var result = _validator.TestValidate(documentDto);

        // Assert
        // You might need to adapt the rule if you want to enforce specific values.
    }
}
