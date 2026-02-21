namespace DocumentManagement.Application.DTOs;

public class UpdateDocumentDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
}
