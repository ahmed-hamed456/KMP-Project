namespace DocumentManagement.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
