using DocumentManagement.Domain.Entities;

namespace DocumentManagement.Domain.Interfaces;

public interface IDocumentRepository
{
    Task<(List<Document> Documents, int TotalCount)> GetAllAsync(
        int page, 
        int pageSize, 
        string? category, 
        Guid? userDepartmentId, 
        string userRole);
    
    Task<Document?> GetByIdAsync(Guid id, Guid? userDepartmentId, string userRole);
    Task<Document> CreateAsync(Document document);
    Task<Document> UpdateAsync(Document document);
    Task<bool> DeleteAsync(Guid id, Guid? userDepartmentId, string userRole);
}
