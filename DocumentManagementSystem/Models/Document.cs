namespace DocumentManagementSystem.Models
{
    public class Document
    {
        public long Id {  get; set; }

        public Document(long id)
        {
            Id = id;
        }
    }
}
