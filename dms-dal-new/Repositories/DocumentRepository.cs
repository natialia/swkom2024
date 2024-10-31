using dms_dal_new.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dms_dal_new.Data;
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
            return await _context.DocumentItems!.ToListAsync();
        }

        public async Task<DocumentItem> GetByIdAsync(int id)
        {
            return (await _context.DocumentItems!.FindAsync(id))!;
        }

        public async Task AddAsync(DocumentItem item)
        {
            await _context.DocumentItems!.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DocumentItem item)
        {
            _context.DocumentItems!.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _context.DocumentItems!.FindAsync(id);
            if (item != null)
            {
                _context.DocumentItems.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ContainsItem(int id)
        {
            return await GetByIdAsync(id) != null;
        }
    }
}
