using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dms_bl.Models;

namespace dms_bl.Services
{
    public interface IDocumentService
    {
        Task<Document?> GetDocumentByIdAsync(int id);
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task<ServiceResponse> AddDocumentAsync(Document item);
        Task<ServiceResponse> UpdateDocumentAsync(int id, Document item);
        Task<ServiceResponse> DeleteAsync(int id);
    }
}
