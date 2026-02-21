using System.Security.Claims;
using DocumentManagement.Application.DTOs;
using DocumentManagement.Application.Interfaces;
using DocumentManagement.Domain.Common;
using DocumentManagement.Domain.Entities;
using DocumentManagement.Domain.Interfaces;

namespace DocumentManagement.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _repository;
    
    public DocumentService(IDocumentRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<PagedResultDto<DocumentDto>> GetDocumentsAsync(int page, int pageSize, string? category, ClaimsPrincipal user)
    {
        var (userId, role, departmentId) = ExtractUserInfo(user);
        
        // Validate and constrain pagination
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        
        var (documents, totalCount) = await _repository.GetAllAsync(page, pageSize, category, departmentId, role);
        
        return new PagedResultDto<DocumentDto>
        {
            Items = documents.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
    
    public async Task<DocumentDto?> GetDocumentByIdAsync(Guid id, ClaimsPrincipal user)
    {
        var (userId, role, departmentId) = ExtractUserInfo(user);
        
        var document = await _repository.GetByIdAsync(id, departmentId, role);
        
        return document != null ? MapToDto(document) : null;
    }
    
    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto dto, ClaimsPrincipal user)
    {
        var (userId, role, departmentId) = ExtractUserInfo(user);
        
        // Create a sample file path based on category (files should be pre-seeded)
        var fileName = $"{dto.Category.ToLower()}-{Guid.CreateVersion7()}.pdf";
        
        var document = new Document
        {
            Id = Guid.CreateVersion7(),
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Tags = dto.Tags ?? new List<string>(),
            FilePath = fileName,
            FileSize = 524288, // Mock file size
            MimeType = "application/pdf", // Mock MIME type
            DepartmentId = dto.DepartmentId,
            CreatedBy = userId
        };
        
        var created = await _repository.CreateAsync(document);
        return MapToDto(created);
    }
    
    public async Task<DocumentDto?> UpdateDocumentAsync(Guid id, UpdateDocumentDto dto, ClaimsPrincipal user)
    {
        var (userId, role, departmentId) = ExtractUserInfo(user);
        
        var document = await _repository.GetByIdAsync(id, departmentId, role);
        if (document == null)
        {
            return null;
        }
        
        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(dto.Title))
            document.Title = dto.Title;
        
        if (dto.Description != null)
            document.Description = dto.Description;
        
        if (!string.IsNullOrWhiteSpace(dto.Category))
            document.Category = dto.Category;
        
        if (dto.Tags != null)
            document.Tags = dto.Tags;
        
        var updated = await _repository.UpdateAsync(document);
        return MapToDto(updated);
    }
    
    public async Task<bool> DeleteDocumentAsync(Guid id, ClaimsPrincipal user)
    {
        var (userId, role, departmentId) = ExtractUserInfo(user);
        
        return await _repository.DeleteAsync(id, departmentId, role);
    }
    
    private (Guid userId, string role, Guid? departmentId) ExtractUserInfo(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? user.FindFirst("sub")?.Value;
        var userId = Guid.Parse(userIdClaim ?? Guid.Empty.ToString());
        
        var role = user.FindFirst(ClaimTypes.Role)?.Value 
                   ?? user.FindFirst("role")?.Value 
                   ?? Roles.Viewer;
        
        var departmentIdClaim = user.FindFirst("departmentId")?.Value;
        Guid? departmentId = null;
        if (!string.IsNullOrEmpty(departmentIdClaim) && Guid.TryParse(departmentIdClaim, out var deptId))
        {
            departmentId = deptId;
        }
        
        return (userId, role, departmentId);
    }
    
    private DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            Category = document.Category,
            Tags = document.Tags,
            FilePath = document.FilePath,
            FileSize = document.FileSize,
            MimeType = document.MimeType,
            DepartmentId = document.DepartmentId,
            DepartmentName = document.Department?.Name ?? string.Empty,
            CreatedBy = document.CreatedBy,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };
    }
}
