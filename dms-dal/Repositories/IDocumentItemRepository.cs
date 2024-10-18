using dms_dal.Entities;

namespace dms_dal.Repositories
{
    public interface IDocumentItemRepository
    {
        Task<IEnumerable<DocumentItem>> GetAllAsync();
        Task<DocumentItem> GetByIdAsync(int id);
        Task AddAsync(DocumentItem item);
        Task UpdateAsync(DocumentItem item);
        Task DeleteAsync(int id);
    }
}
