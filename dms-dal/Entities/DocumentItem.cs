namespace dms_dal.Entities
{
    public class DocumentItem(long id)
    {
        public long Id { get; set; } = id;
        public string Name { get; set; }
        public bool? IsComplete { get; set; }
    }
}
