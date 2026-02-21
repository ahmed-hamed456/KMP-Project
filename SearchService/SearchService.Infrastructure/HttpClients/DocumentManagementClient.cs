using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SearchService.Domain.Interfaces;

namespace SearchService.Infrastructure.HttpClients;

public class DocumentManagementClient : IDocumentManagementClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentManagementClient> _logger;

    public DocumentManagementClient(HttpClient httpClient, ILogger<DocumentManagementClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentSyncDto>> GetAllDocumentsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching documents from DocumentManagement API...");

            // Call the DocumentManagement API to get all documents
            // The API endpoint returns paginated results, so we need to get all pages
            var allDocuments = new List<DocumentResponseDto>();
            var page = 1;
            var pageSize = 100;
            bool hasMorePages;

            do
            {
                var response = await _httpClient.GetAsync($"/api/documents?page={page}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();

                var pagedResult = await response.Content.ReadFromJsonAsync<PagedDocumentResponse>();
                
                if (pagedResult?.Data != null)
                {
                    allDocuments.AddRange(pagedResult.Data);
                    hasMorePages = page < pagedResult.TotalPages;
                    page++;
                }
                else
                {
                    hasMorePages = false;
                }

            } while (hasMorePages);

            _logger.LogInformation("Fetched {Count} documents from DocumentManagement API", allDocuments.Count);

            // Map to sync DTOs
            return allDocuments.Select(d => new DocumentSyncDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                Category = d.Category,
                Tags = d.Tags,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                IsDeleted = d.IsDeleted,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching documents from DocumentManagement API");
            throw;
        }
    }

    // DTOs matching DocumentManagement API response
    private class PagedDocumentResponse
    {
        public List<DocumentResponseDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    private class DocumentResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
