using System.Security.Claims;
using DocumentManagement.Application.DTOs;

namespace DocumentManagement.Application.Interfaces;

public interface IDocumentService
{
    Task<PagedResultDto<DocumentDto>> GetDocumentsAsync(int page, int pageSize, string? category, ClaimsPrincipal user);
    Task<DocumentDto?> GetDocumentByIdAsync(Guid id, ClaimsPrincipal user);
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto dto, ClaimsPrincipal user);
    Task<DocumentDto?> UpdateDocumentAsync(Guid id, UpdateDocumentDto dto, ClaimsPrincipal user);
    Task<bool> DeleteDocumentAsync(Guid id, ClaimsPrincipal user);
}
