using dms_bl.Models;
using dms_bl.Validators;
using FluentValidation.TestHelper;
using Xunit;

public class DocumentValidatorTests
{
    private readonly DocumentValidator _validator; // Instance of the DocumentValidator for testing

    // Constructor to set up the validator instance
    public DocumentValidatorTests()
    {
        _validator = new DocumentValidator(); // Initialize the validator
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Is_Empty()
    {
        var document = new Document { FileSize = "" }; // Create a Document with empty FileSize
        var result = _validator.TestValidate(document); // Validate the document
        result.ShouldHaveValidationErrorFor(x => x.FileSize); // Assert that there is a validation error for FileSize
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Exceeds_Limit()
    {
        var document = new Document { FileSize = "15000kB" }; // Create a Document with FileSize exceeding the limit
        var result = _validator.TestValidate(document); // Validate the document
        result.ShouldHaveValidationErrorFor(x => x.FileSize); // Assert that there is a validation error for FileSize
    }

    [Fact]
    public void Should_Not_Have_Error_When_FileSize_Is_Within_Limit()
    {
        var document = new Document { FileSize = "5000kB" }; // Create a Document with FileSize within the limit
        var result = _validator.TestValidate(document); // Validate the document
        result.ShouldNotHaveValidationErrorFor(x => x.FileSize); // Assert that there is no validation error for FileSize
    }

    [Fact]
    public void Should_Have_Error_When_FileSize_Has_Invalid_Format()
    {
        var document = new Document { FileSize = "5MB" }; // Create a Document with an invalid FileSize format
        var result = _validator.TestValidate(document); // Validate the document
        result.ShouldHaveValidationErrorFor(x => x.FileSize); // Assert that there is a validation error for FileSize
    }
}