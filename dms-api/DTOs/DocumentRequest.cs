using DocumentManagementSystem.DTOs;

namespace dms_api.DTOs
{
    public class DocumentRequest
    {
        public IFormFile UploadedDocument { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
    }
}
