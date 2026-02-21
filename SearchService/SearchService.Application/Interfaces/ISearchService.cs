using SearchService.Application.DTOs;

namespace SearchService.Application.Interfaces;

public interface ISearchService
{
    Task<SearchResponseDto> SearchAsync(SearchRequestDto request, string userRole, Guid? userDepartmentId);
    Task<SuggestionsResponseDto> GetSuggestionsAsync(string prefix, int limit = 10);
    Task<FacetsDto> GetFacetsAsync();
}
