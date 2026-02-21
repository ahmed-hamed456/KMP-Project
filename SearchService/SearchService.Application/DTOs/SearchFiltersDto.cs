namespace SearchService.Application.DTOs;

public class SearchFiltersDto
{
    public List<string>? Categories { get; set; }
    public List<Guid>? DepartmentIds { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
