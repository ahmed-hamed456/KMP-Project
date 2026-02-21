using SearchService.Domain.Entities;

namespace SearchService.Domain.Interfaces;

public interface ISearchRepository
{
    Task<(IEnumerable<(SearchableDocument doc, double rank)> Results, int TotalCount)> SearchAsync(
        string query,
        IEnumerable<string>? categories,
        IEnumerable<Guid>? departmentIds,
        DateTime? fromDate,
        DateTime? toDate,
        string sortBy,
        bool ascending,
        int page,
        int pageSize,
        string userRole,
        Guid? userDepartmentId);

    Task<IEnumerable<string>> GetSuggestionsAsync(string prefix, int limit);
    
    Task<Dictionary<string, IEnumerable<object>>> GetFacetsAsync();
    
    Task<SearchableDocument?> GetByIdAsync(Guid id);
    
    Task<IEnumerable<SearchableDocument>> GetAllAsync();
    
    Task UpsertAsync(SearchableDocument document);
    
    Task DeleteAsync(Guid id);
}
