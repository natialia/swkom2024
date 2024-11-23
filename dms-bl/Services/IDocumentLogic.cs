using dms_bl.Models;
using dms_dal_new.Entities;

namespace dms_bl.Services
{
    public interface IDocumentLogic
    {
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<DocumentItem?> AddDocumentAsync(Document item);
        Task<ServiceResponse> UpdateDocumentAsync(int id, Document item);
        Task<ServiceResponse> DeleteAsync(int id);
    }
}
