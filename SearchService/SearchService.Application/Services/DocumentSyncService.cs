using Microsoft.Extensions.Logging;
using SearchService.Application.Interfaces;
using SearchService.Domain.Entities;
using SearchService.Domain.Interfaces;

namespace SearchService.Application.Services;

public class DocumentSyncService : IDocumentSyncService
{
    private readonly IDocumentManagementClient _documentClient;
    private readonly ISearchRepository _searchRepository;
    private readonly ILogger<DocumentSyncService> _logger;

    public DocumentSyncService(
        IDocumentManagementClient documentClient,
        ISearchRepository searchRepository,
        ILogger<DocumentSyncService> logger)
    {
        _documentClient = documentClient;
        _searchRepository = searchRepository;
        _logger = logger;
    }

    public async Task SyncDocumentsAsync()
    {
        try
        {
            _logger.LogInformation("Starting document synchronization...");

            var sourceDocuments = await _documentClient.GetAllDocumentsAsync();
            var syncedCount = 0;

            foreach (var sourceDoc in sourceDocuments)
            {
                var searchableDoc = new SearchableDocument
                {
                    Id = sourceDoc.Id,
                    Title = sourceDoc.Title,
                    Description = sourceDoc.Description,
                    Category = sourceDoc.Category,
                    Tags = string.Join(", ", sourceDoc.Tags),
                    DepartmentId = sourceDoc.DepartmentId,
                    DepartmentName = sourceDoc.DepartmentName,
                    IsDeleted = sourceDoc.IsDeleted,
                    CreatedAt = sourceDoc.CreatedAt,
                    UpdatedAt = sourceDoc.UpdatedAt,
                    LastSyncedAt = DateTime.UtcNow
                };

                if (sourceDoc.IsDeleted)
                {
                    await _searchRepository.DeleteAsync(sourceDoc.Id);
                    _logger.LogDebug("Deleted document {DocumentId}", sourceDoc.Id);
                }
                else
                {
                    await _searchRepository.UpsertAsync(searchableDoc);
                    syncedCount++;
                }
            }

            _logger.LogInformation("Document synchronization completed. Synced {Count} documents.", syncedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document synchronization");
            throw;
        }
    }
}
