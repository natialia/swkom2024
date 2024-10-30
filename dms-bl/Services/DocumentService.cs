using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dms_bl.Models;

namespace dms_bl.Services
{
    public class DocumentService
    {
        private readonly IDocumentRepository _documentRepository;

        public DocumentService(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository; // Injected in api
        }

        public async Task<Document> GetDocumentByIdAsync(int id)
        {

        }
    }
}
