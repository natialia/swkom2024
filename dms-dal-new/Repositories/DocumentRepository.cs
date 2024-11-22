using dms_dal_new.Entities;
using dms_dal_new.Data;
using dms_dal_new.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace dms_dal_new.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocumentContext _context;

        public DocumentRepository(DocumentContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DocumentItem>> GetAllAsync()
        {
            try
            {
                return await _context.DocumentItems!.ToListAsync();
            }
            catch(Exception ex)
            {
                throw new DataAccessException("Error retrieving all documents in DAL", ex);
            }
        }

        public async Task<DocumentItem> GetByIdAsync(int id)
        {
            try
            {
                return (await _context.DocumentItems!.FindAsync(id))!;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Error retrieving document by ID in DAL", ex);
            }
        }

        public async Task AddAsync(DocumentItem item)
        {
            try
            {
                await _context.DocumentItems!.AddAsync(item); //Tells the compiler it isnt null for sure
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Error adding new document in DAL", ex);
            }
        }

        public async Task UpdateAsync(DocumentItem item)
        {
            try
            {
                _context.DocumentItems!.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Error updating document in DAL", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var item = await _context.DocumentItems!.FindAsync(id);
                if (item != null)
                {
                    _context.DocumentItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Error deleting document in DAL", ex);
            }
        }

        public async Task<bool> ContainsItem(int id)
        {
            try
            {
                return await GetByIdAsync(id) != null;
            }
            catch (Exception ex)
            {
                throw new DataAccessException("Error checking if document exists in DAL", ex);
            }
        }
    }
}
