using AutoMapper;
using dms_bl.Models;
using dms_dal_new.Entities;
using dms_dal_new.Repositories;
using dms_bl.Exceptions;
using FluentValidation;
using dms_dal_new.Exceptions;

namespace dms_bl.Services
{
    public class DocumentLogic: IDocumentLogic
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<Document> _documentValidator;

        public DocumentLogic(IDocumentRepository documentRepository, IMapper mapper, IValidator<Document> documentValidator)
        {
            _documentRepository = documentRepository; // Injected in api
            _mapper = mapper;
            _documentValidator = documentValidator;
        }

        public async Task<Document?> GetDocumentByIdAsync(int id)
        {
            try
            {
                var item = await _documentRepository.GetByIdAsync(id); // Returns null if not found: check in api
                if (item == null) return null;

                var document = _mapper.Map<Document>(item);
                return document;
            }
            catch(Exception ex)
            {
                throw new BusinessException("Error getting document by ID in BL", ex);
            }
        }

        public async Task<IEnumerable<Document>> GetAllDocumentsAsync()
        {
            try
            {
                var items = await _documentRepository.GetAllAsync();

                var documents = _mapper.Map<IEnumerable<Document>>(items); // Map entities to Documents
                return documents; // Return the mapped DTOs
            }
            catch(Exception ex)
            {
                throw new BusinessException("Error getting all documents in BL", ex);
            }
        }

        public async Task<DocumentItem?> AddDocumentAsync(Document item)
        {
            try
            {
                var validator = _documentValidator.Validate(item);
                if (!validator.IsValid)
                {
                    return null;
                }
                var documentItem = _mapper.Map<DocumentItem>(item);
                var addedDocument = await _documentRepository.AddAsync(documentItem);

                return addedDocument;
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error adding document in BL", ex);
            }
        }

        public async Task<ServiceResponse> UpdateDocumentAsync(int id, Document item)
        {
            try
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
                existingItem.OcrText = item.OcrText;
                await _documentRepository.UpdateAsync(existingItem);
                return new ServiceResponse { Success = true, Message = "Successfully updated document" };
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error updating document in BL", ex);
            }
        }

        public async Task<ServiceResponse> DeleteAsync(int id)
        {
            try
            {
                if (!_documentRepository.ContainsItem(id).Result)
                {
                    return new ServiceResponse { Success = false, Message = "Could not delete: does not exist" };
                }
                await _documentRepository.DeleteAsync(id);
                return new ServiceResponse { Success = true, Message = "Successfully deleted document" };
            }
            catch (Exception ex)
            {
                throw new BusinessException("Error deleting document in BL", ex);
            }
        }
    }
}
