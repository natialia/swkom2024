using AutoMapper;
using dms_bl.Models;
using dms_dal_new.Entities;
using dms_dal_new.Repositories;
using FluentValidation;

namespace dms_bl.Services
{
    public class DocumentService: IDocumentService
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<Document> _documentValidator;

        public DocumentService(IDocumentRepository documentRepository, IMapper mapper, IValidator<Document> documentValidator)
        {
            _documentRepository = documentRepository; // Injected in api
            _mapper = mapper;
            _documentValidator = documentValidator;
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            var item = await _documentRepository.GetByIdAsync(id); //returns null if not found: check in api
            if (item == null) return null;

            var document = _mapper.Map<Document>(item);
            return document;
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            var items = await _documentRepository.GetAllAsync();
            
            var documents = _mapper.Map<IEnumerable<Document>>(items); // Map entities to Documents
            return documents; // Return the mapped DTOs
        }

        public async Task<ServiceResponse> AddDocumentAsync(Document item)
        {
            var validator = _documentValidator.Validate(item);
            if (!validator.IsValid)
            {
                return new ServiceResponse { Success = false, Message = "Could not add: Invalid Document Object" };
            }
            var documentItem = _mapper.Map<DocumentItem>(item);
            await _documentRepository.AddAsync(documentItem);

            return new ServiceResponse { Success = true, Message = "Successfully added document" };
        }

        public async Task<ServiceResponse> UpdateDocumentAsync(int id, Document item)
        {
            var validator = _documentValidator.Validate(item);
            if (!validator.IsValid)
            {
                return new ServiceResponse { Success = false, Message = "Could not update: Invalid Document Object" };
            }

            var existingItem = await _documentRepository.GetByIdAsync(id);
            if (existingItem == null)
            {
                return new ServiceResponse { Success = false, Message = "Could not update: does not exist" };
            }

            existingItem.Name = item.Name;
            existingItem.FileType = item.FileType;
            existingItem.FileSize = item.FileSize;
            await _documentRepository.UpdateAsync(existingItem);
            return new ServiceResponse { Success = true, Message = "Successfully updated document" };
        }

        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            if (!_documentRepository.ContainsItem(id).Result)
            {
                return new ServiceResponse { Success = false, Message = "Could not delete: does not exist" };
            }
            await _documentRepository.DeleteAsync(id);
            return new ServiceResponse { Success = true, Message = "Successfully deleted document" };
        }
    }
}
