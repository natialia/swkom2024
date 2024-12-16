using System.Diagnostics.CodeAnalysis;

namespace dms_bl.Models
{
    [ExcludeFromCodeCoverage]
    public class ServiceResponse
    {
        public bool Success { get; set; } = true;
        public string? Message { get; set; } = null;
    }
}
