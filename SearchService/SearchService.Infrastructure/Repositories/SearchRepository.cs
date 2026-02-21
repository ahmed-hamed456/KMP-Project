using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SearchService.Domain.Entities;
using SearchService.Domain.Interfaces;
using SearchService.Infrastructure.Data;
using System.Text;

namespace SearchService.Infrastructure.Repositories;

public class SearchRepository : ISearchRepository
{
    private readonly SearchDbContext _context;

    public SearchRepository(SearchDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<(SearchableDocument doc, double rank)> Results, int TotalCount)> SearchAsync(
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
        Guid? userDepartmentId)
    {
        // Format search query for CONTAINSTABLE - use OR logic for multi-word queries
        var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var ftsQuery = string.Join(" OR ", searchTerms.Select(t => $"\"{t}*\""));

        // Build SQL query with CONTAINSTABLE for Full-Text Search
        var sqlBuilder = new StringBuilder();
        var parameters = new List<SqlParameter>();

        // Base query with CONTAINSTABLE join
        sqlBuilder.Append(@"
            SELECT d.*, fts.[RANK] AS Score
            FROM CONTAINSTABLE(SearchableDocuments, (Title, Description, Tags), @searchTerm) AS fts
            INNER JOIN SearchableDocuments d ON fts.[KEY] = d.Id
            WHERE d.IsDeleted = 0");

        // Add search term parameter
        parameters.Add(new SqlParameter("@searchTerm", ftsQuery));

        // Apply role-based filtering
        if (userRole?.ToLowerInvariant() == "viewer" && userDepartmentId.HasValue)
        {
            sqlBuilder.Append(" AND d.DepartmentId = @userDepartmentId");
            parameters.Add(new SqlParameter("@userDepartmentId", userDepartmentId.Value));
        }

        // Apply category filter 
        if (categories?.Any() == true)
        {
            var categoryList = string.Join("','", categories.Select(c => c.Replace("'", "''")));
            sqlBuilder.Append($" AND d.Category IN ('{categoryList}')");
        }

        // Apply department filter
        if (departmentIds?.Any() == true)
        {
            var deptIdList = string.Join("','", departmentIds.Select(d => d.ToString()));
            sqlBuilder.Append($" AND d.DepartmentId IN ('{deptIdList}')");
        }

        // Apply date range filters
        if (fromDate.HasValue)
        {
            sqlBuilder.Append(" AND d.CreatedAt >= @fromDate");
            parameters.Add(new SqlParameter("@fromDate", fromDate.Value));
        }

        if (toDate.HasValue)
        {
            sqlBuilder.Append(" AND d.CreatedAt <= @toDate");
            parameters.Add(new SqlParameter("@toDate", toDate.Value));
        }

        // Get total count before pagination
        var countSql = $"SELECT COUNT(*) FROM ({sqlBuilder}) AS CountQuery";
        int totalCount;
        
        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync();
        }
        
        using (var command = connection.CreateCommand())
        {
            command.CommandText = countSql;
            foreach (var param in parameters)
            {
                var clonedParam = new SqlParameter(param.ParameterName, param.Value ?? DBNull.Value);
                command.Parameters.Add(clonedParam);
            }
            totalCount = (int)(await command.ExecuteScalarAsync() ?? 0);
        }

        // Apply sorting
        sqlBuilder.Append(sortBy.ToLowerInvariant() switch
        {
            "createdat" => ascending ? " ORDER BY d.CreatedAt ASC" : " ORDER BY d.CreatedAt DESC",
            "date" => ascending ? " ORDER BY d.CreatedAt ASC" : " ORDER BY d.CreatedAt DESC",
            "updatedat" => ascending ? " ORDER BY d.UpdatedAt ASC" : " ORDER BY d.UpdatedAt DESC",
            "title" => ascending ? " ORDER BY d.Title ASC" : " ORDER BY d.Title DESC",
            "relevance" or _ => " ORDER BY fts.[RANK] DESC"
        });

        // Apply pagination
        var offset = (page - 1) * pageSize;
        sqlBuilder.Append(" OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY");
        parameters.Add(new SqlParameter("@offset", offset));
        parameters.Add(new SqlParameter("@pageSize", pageSize));

        // Execute query using raw SQL
        var sql = sqlBuilder.ToString();
        var results = new List<(SearchableDocument doc, double rank)>();
        
        try
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = sql;
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var doc = new SearchableDocument
                        {
                            Id = reader.GetGuid(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                            Category = reader.GetString(reader.GetOrdinal("Category")),
                            Tags = reader.GetString(reader.GetOrdinal("Tags")),
                            DepartmentId = reader.GetGuid(reader.GetOrdinal("DepartmentId")),
                            DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                            LastSyncedAt = reader.GetDateTime(reader.GetOrdinal("LastSyncedAt")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                        };
                        
                        var rank = reader.GetInt32(reader.GetOrdinal("Score"));
                        results.Add((doc, (double)rank));
                    }
                }
            }
        }
        finally
        {
            // Don't dispose the connection - let EF Core manage it
        }

        return (results, totalCount);
    }

    public async Task<IEnumerable<string>> GetSuggestionsAsync(string prefix, int limit)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            return Enumerable.Empty<string>();

        var suggestions = await _context.SearchableDocuments
            .Where(d => !d.IsDeleted && d.Title.StartsWith(prefix))
            .Select(d => d.Title)
            .Distinct()
            .Take(limit)
            .ToListAsync();

        return suggestions;
    }

    public async Task<Dictionary<string, IEnumerable<object>>> GetFacetsAsync()
    {
        var categoryFacets = await _context.SearchableDocuments
            .Where(d => !d.IsDeleted)
            .GroupBy(d => d.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var departmentFacets = await _context.SearchableDocuments
            .Where(d => !d.IsDeleted)
            .GroupBy(d => new { d.DepartmentId, d.DepartmentName })
            .Select(g => new { g.Key.DepartmentId, g.Key.DepartmentName, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return new Dictionary<string, IEnumerable<object>>
        {
            { "categories", categoryFacets.Cast<object>() },
            { "departments", departmentFacets.Cast<object>() }
        };
    }

    public async Task<SearchableDocument?> GetByIdAsync(Guid id)
    {
        return await _context.SearchableDocuments.FindAsync(id);
    }

    public async Task<IEnumerable<SearchableDocument>> GetAllAsync()
    {
        return await _context.SearchableDocuments.ToListAsync();
    }

    public async Task UpsertAsync(SearchableDocument document)
    {
        var existing = await _context.SearchableDocuments.FindAsync(document.Id);

        if (existing != null)
        {
            _context.Entry(existing).CurrentValues.SetValues(document);
        }
        else
        {
            await _context.SearchableDocuments.AddAsync(document);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var document = await _context.SearchableDocuments.FindAsync(id);
        if (document != null)
        {
            _context.SearchableDocuments.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
