﻿using dms_dal_new.Entities;

namespace dms_dal_new.Repositories
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<DocumentItem>> GetAllAsync();
        Task<DocumentItem> GetByIdAsync(int id);
        Task<DocumentItem?> AddAsync(DocumentItem item);
        Task UpdateAsync(DocumentItem item);
        Task DeleteAsync(int id);
        Task<bool> ContainsItem(int id);
    }
}
