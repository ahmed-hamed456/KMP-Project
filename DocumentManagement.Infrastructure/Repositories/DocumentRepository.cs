using Microsoft.EntityFrameworkCore;
using DocumentManagement.Domain.Common;
using DocumentManagement.Domain.Entities;
using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data;

namespace DocumentManagement.Infrastructure.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _context;
    
    public DocumentRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<(List<Document> Documents, int TotalCount)> GetAllAsync(
        int page, 
        int pageSize, 
        string? category, 
        Guid? userDepartmentId, 
        string userRole)
    {
        var query = _context.Documents
            .Include(d => d.Department)
            .AsQueryable();
        
        // Apply role-based filtering
        if (userRole == Roles.Viewer && userDepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == userDepartmentId.Value);
        }
        // Admin and Editor see all documents (no additional filter needed)
        
        // Apply category filter if provided
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(d => d.Category == category);
        }
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return (documents, totalCount);
    }
    
    public async Task<Document?> GetByIdAsync(Guid id, Guid? userDepartmentId, string userRole)
    {
        var query = _context.Documents
            .Include(d => d.Department)
            .Where(d => d.Id == id);
        
        // Apply role-based filtering
        if (userRole == Roles.Viewer && userDepartmentId.HasValue)
        {
            query = query.Where(d => d.DepartmentId == userDepartmentId.Value);
        }
        
        return await query.FirstOrDefaultAsync();
    }
    
    public async Task<Document> CreateAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }
    
    public async Task<Document> UpdateAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }
    
    public async Task<bool> DeleteAsync(Guid id, Guid? userDepartmentId, string userRole)
    {
        var document = await GetByIdAsync(id, userDepartmentId, userRole);
        if (document == null)
        {
            return false;
        }
        
        _context.Documents.Remove(document); // Will be intercepted by SaveChangesAsync and soft-deleted
        await _context.SaveChangesAsync();
        return true;
    }
}
