namespace SearchService.Application.DTOs;

public class SearchResponseDto
{
    public List<SearchResultDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public FacetsDto? Facets { get; set; }
}
