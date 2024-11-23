namespace DocumentManagementSystem.DTOs
{
    public class DocumentDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? FileType { get; set; }
        public string? FileSize { get; set; }
        public string? OcrText { get; set; }
    }
}
