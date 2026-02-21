namespace DocumentManagement.Application.DTOs;

public class CreateDocumentDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public Guid DepartmentId { get; set; }
}
