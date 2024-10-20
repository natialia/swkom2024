namespace dms_dal.Entities
{
    public class DocumentItem(int id)
    {
        public int Id { get; set; } = id;
        public string? Name { get; set; }
        public bool? IsComplete { get; set; }
    }
}
