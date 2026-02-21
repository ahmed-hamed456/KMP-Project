namespace SearchService.Application.DTOs;

public class FacetsDto
{
    public List<CategoryFacetDto> Categories { get; set; } = new();
    public List<DepartmentFacetDto> Departments { get; set; } = new();
}

public class CategoryFacetDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DepartmentFacetDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int Count { get; set; }
}
