namespace SearchService.Application.DTOs;

public class SearchRequestDto
{
    public string Query { get; set; } = string.Empty;
    public SearchFiltersDto? Filters { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "relevance";
    public bool Ascending { get; set; } = false;
}
