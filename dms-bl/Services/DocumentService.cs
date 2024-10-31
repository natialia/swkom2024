using AutoMapper;
using dms_bl.Models;
using dms_dal_new.Entities;
using dms_dal_new.Repositories;

namespace dms_bl.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;

        public DocumentService(IDocumentRepository documentRepository, IMapper mapper)
        {
            _documentRepository = documentRepository; // Injected in api
            _mapper = mapper;
        }

        public async Task<Document> GetDocumentByIdAsync(int id)
        {
            return null;
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            var items = _documentRepository.GetAllAsync();
            
            var documents = _mapper.Map<IEnumerable<Document>>(items); // Map entities to Documents
            return documents; // Return the mapped DTOs
        }

        public async Task<bool> AddDocumentAsync(Document item)
        {
            var documentItem = _mapper.Map<DocumentItem>(item);
            _documentRepository.AddAsync(documentItem);

            return true;
        }

        public async Task UpdateDocumentAsync(Document item)
        {
            //if !repository.Contains ..
        }

        public async Task DeleteAsync(int id)
        {
            //if !repository.Contains ..
        }
    }
}
