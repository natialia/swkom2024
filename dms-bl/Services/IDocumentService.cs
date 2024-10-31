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
        Task<Document> GetDocumentByIdAsync(int id);
        Task<IEnumerable<Document>> GetAllDocumentsAsync();
        Task AddDocumentAsync(Document item);
        Task UpdateDocumentAsync(Document item);
        Task DeleteAsync(int id);
    }
}
