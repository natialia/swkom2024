namespace dms_bl.Models
{
    /// <summary>
    /// Represents a document for processing in services.
    /// </summary>
    public class Document
    {
        /// <summary>
        /// The unique ID of the document.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the document.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The type of the document (e.g., PDF, Word).
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// The size of the document in bytes.
        /// </summary>
        public string? FileSize { get; set; }

        /// <summary>
        /// The extracted OCR Text of the document.
        /// </summary>
        public string? OcrText { get; set; }
    }
}
