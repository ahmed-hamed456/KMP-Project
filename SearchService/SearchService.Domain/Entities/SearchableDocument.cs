namespace SearchService.Domain.Entities;

public class SearchableDocument : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty; // Stored as comma-separated for FTS
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime LastSyncedAt { get; set; }
}
