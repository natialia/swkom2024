using DocumentManagementSystem.DTOs;
using FluentValidation.TestHelper;
using Xunit;

public class DocumentDTOValidatorTests
{
    private readonly DocumentDTOValidator _validator;

    public DocumentDTOValidatorTests()
    {
        _validator = new DocumentDTOValidator(); // Initialize the validator
    }
    //TODO: 75% test coveragem use coverage tool
    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var documentDto = new DocumentDTO { Name = "" }; // Create a DocumentDTO with an empty Name
        var result = _validator.TestValidate(documentDto); // Validate the document DTO
        result.ShouldHaveValidationErrorFor(x => x.Name); // Assert that there is a validation error for Name
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        var documentDto = new DocumentDTO { Name = new string('A', 256) }; // Create a DocumentDTO with a Name exceeding 100 characters
        var result = _validator.TestValidate(documentDto); // Validate the document DTO
        result.ShouldHaveValidationErrorFor(x => x.Name); // Assert that there is a validation error for Name
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var documentDto = new DocumentDTO { Name = "validName.pdf" }; // Create a DocumentDTO with a valid Name
        var result = _validator.TestValidate(documentDto); // Validate the document DTO
        result.ShouldNotHaveValidationErrorFor(x => x.Name); // Assert that there is no validation error for Name
    }
}
