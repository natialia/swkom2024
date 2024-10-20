namespace dms_dal.Entities
{
    public class DocumentItem
    {
        // Parameterless constructor
        public DocumentItem() { }

        // Constructor with 'id'
        public DocumentItem(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public bool? IsComplete { get; set; }
    }
}
