using SearchService.Domain.Entities;

namespace SearchService.Domain.Interfaces;

public interface IDocumentManagementClient
{
    Task<IEnumerable<DocumentSyncDto>> GetAllDocumentsAsync();
}

public class DocumentSyncDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
