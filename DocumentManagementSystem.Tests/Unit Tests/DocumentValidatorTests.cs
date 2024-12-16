using dms_bl.Models;
using dms_bl.Validators;
using FluentValidation.TestHelper;
using Xunit;

public class DocumentValidatorTests
{
    private readonly DocumentValidator _validator;

    // Constructor to initialize the validator instance
    public DocumentValidatorTests()
    {
        // Arrange: Initialize the validator
        _validator = new DocumentValidator();
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Is_Empty()
    {
        // Arrange: Prepare a Document with an empty FileSize
        var document = new Document { FileSize = "" };

        // Act: Validate the document
        var result = _validator.TestValidate(document);

        // Assert: Verify that a validation error occurs for FileSize
        result.ShouldHaveValidationErrorFor(x => x.FileSize);
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Exceeds_Limit()
    {
        // Arrange: Prepare a Document with FileSize exceeding the limit
        var document = new Document { FileSize = "15000kB" };

        // Act: Validate the document
        var result = _validator.TestValidate(document);

        // Assert: Verify that a validation error occurs for FileSize
        result.ShouldHaveValidationErrorFor(x => x.FileSize);
    }

    [Fact]
    public void Should_Not_Have_Error_When_FileSize_Is_Within_Limit()
    {
        // Arrange: Prepare a Document with FileSize within the limit
        var document = new Document { FileSize = "5000kB" };

        // Act: Validate the document
        var result = _validator.TestValidate(document);

        // Assert: Verify that no validation error occurs for FileSize
        result.ShouldNotHaveValidationErrorFor(x => x.FileSize);
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Has_Invalid_Format()
    {
        // Arrange: Prepare a Document with an invalid FileSize format
        var document = new Document { FileSize = "5MB" };

        // Act: Validate the document
        var result = _validator.TestValidate(document);

        // Assert: Verify that a validation error occurs for FileSize
        result.ShouldHaveValidationErrorFor(x => x.FileSize);
    }
}
