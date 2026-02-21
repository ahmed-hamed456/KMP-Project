using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using SearchService.Domain.Interfaces;
using FluentValidation;

namespace SearchService.Application.Services;

public class SearchService : ISearchService
{
    private readonly ISearchRepository _repository;

    public SearchService(ISearchRepository repository)
    {
        _repository = repository;
    }

    public async Task<SearchResponseDto> SearchAsync(SearchRequestDto request, string userRole, Guid? userDepartmentId)
    {
        // Validate that search query is not empty
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ValidationException("Search query cannot be empty.");
        }

        var (results, totalCount) = await _repository.SearchAsync(
            request.Query,
            request.Filters?.Categories,
            request.Filters?.DepartmentIds,
            request.Filters?.FromDate,
            request.Filters?.ToDate,
            request.SortBy,
            request.Ascending,
            request.Page,
            request.PageSize,
            userRole,
            userDepartmentId);

        var searchResults = results.Select(result => new SearchResultDto
        {
            Id = result.doc.Id,
            Title = result.doc.Title,
            HighlightedTitle = HighlightText(result.doc.Title, request.Query),
            Description = result.doc.Description,
            HighlightedDescription = result.doc.Description != null ? HighlightText(result.doc.Description, request.Query) : null,
            Category = result.doc.Category,
            Tags = result.doc.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToList(),
            DepartmentId = result.doc.DepartmentId,
            DepartmentName = result.doc.DepartmentName,
            CreatedAt = result.doc.CreatedAt,
            UpdatedAt = result.doc.UpdatedAt,
            Score = result.rank // Map RANK score from repository
        }).ToList();

        return new SearchResponseDto
        {
            Results = searchResults,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<SuggestionsResponseDto> GetSuggestionsAsync(string prefix, int limit = 10)
    {
        var suggestions = await _repository.GetSuggestionsAsync(prefix, limit);
        return new SuggestionsResponseDto
        {
            Suggestions = suggestions.ToList()
        };
    }

    public async Task<FacetsDto> GetFacetsAsync()
    {
        var facets = await _repository.GetFacetsAsync();
        
        var facetsDto = new FacetsDto();

        if (facets.TryGetValue("categories", out var categories))
        {
            // Use reflection to access anonymous type properties
            facetsDto.Categories = categories
                .Select(c => 
                {
                    var type = c.GetType();
                    var categoryProp = type.GetProperty("Category");
                    var countProp = type.GetProperty("Count");
                    
                    return new CategoryFacetDto
                    {
                        Category = categoryProp?.GetValue(c)?.ToString() ?? "",
                        Count = (int)(countProp?.GetValue(c) ?? 0)
                    };
                }).ToList();
        }

        if (facets.TryGetValue("departments", out var departments))
        {
            // Use reflection to access anonymous type properties
            facetsDto.Departments = departments
                .Select(d => 
                {
                    var type = d.GetType();
                    var deptIdProp = type.GetProperty("DepartmentId");
                    var deptNameProp = type.GetProperty("DepartmentName");
                    var countProp = type.GetProperty("Count");
                    
                    return new DepartmentFacetDto
                    {
                        DepartmentId = (Guid)(deptIdProp?.GetValue(d) ?? Guid.Empty),
                        DepartmentName = deptNameProp?.GetValue(d)?.ToString() ?? "",
                        Count = (int)(countProp?.GetValue(d) ?? 0)
                    };
                }).ToList();
        }

        return facetsDto;
    }

    private string HighlightText(string text, string query)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
            return text;

        // Simple case-insensitive highlighting
        var queryTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var highlighted = text;

        foreach (var term in queryTerms)
        {
            if (term.Length < 2) continue;

            // Case-insensitive replacement with <em> tags
            var regex = new System.Text.RegularExpressions.Regex(
                System.Text.RegularExpressions.Regex.Escape(term),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            highlighted = regex.Replace(highlighted, m => $"<em>{m.Value}</em>");
        }

        return highlighted;
    }
}
