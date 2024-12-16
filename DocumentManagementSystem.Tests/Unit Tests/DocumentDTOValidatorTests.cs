using DocumentManagementSystem.DTOs;
using FluentValidation.TestHelper;
using Xunit;

public class DocumentDTOValidatorTests
{
    private readonly DocumentDTOValidator _validator;

    public DocumentDTOValidatorTests()
    {
        // Initialize the validator instance
        _validator = new DocumentDTOValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange: Create a DocumentDTO with an empty Name
        var documentDto = new DocumentDTO { Name = "" };

        // Act: Validate the document DTO
        var result = _validator.TestValidate(documentDto);

        // Assert: Verify that there is a validation error for Name
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Maximum_Length()
    {
        // Arrange: Create a DocumentDTO with a Name exceeding the maximum length
        var documentDto = new DocumentDTO { Name = new string('A', 256) };

        // Act: Validate the document DTO
        var result = _validator.TestValidate(documentDto);

        // Assert: Verify that there is a validation error for Name
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        // Arrange: Create a DocumentDTO with a valid Name
        var documentDto = new DocumentDTO { Name = "validName.pdf" };

        // Act: Validate the document DTO
        var result = _validator.TestValidate(documentDto);

        // Assert: Verify that there is no validation error for Name
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange: Create a DocumentDTO with a null Name
        var documentDto = new DocumentDTO { Name = null };

        // Act: Validate the document DTO
        var result = _validator.TestValidate(documentDto);

        // Assert: Verify that there is a validation error for Name
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FileType_Is_Valid()
    {
        // Arrange: Create a DocumentDTO with a valid FileType
        var documentDto = new DocumentDTO { FileType = "PDF" };

        // Act: Validate the document DTO
        var result = _validator.TestValidate(documentDto);

        // Assert: Verify that there is no validation error for FileType
        result.ShouldNotHaveValidationErrorFor(x => x.FileType);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FileSize_Is_Valid()
    {
        // Arrange: Create a DocumentDTO with a valid FileSize
        var documentDto = new DocumentDTO { FileSize = "2048" };

        // Act: Validate the document DTO
        var result = _validator.TestValidate(documentDto);

        // Assert: Verify that there is no validation error for FileSize
        result.ShouldNotHaveValidationErrorFor(x => x.FileSize);
    }
}
