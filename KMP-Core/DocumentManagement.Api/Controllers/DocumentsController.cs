using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocumentManagement.Application.DTOs;
using DocumentManagement.Application.Interfaces;
using DocumentManagement.Domain.Interfaces;

namespace DocumentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DocumentsController> _logger;
    
    public DocumentsController(
        IDocumentService documentService,
        IFileStorageService fileStorageService,
        ILogger<DocumentsController> logger)
    {
        _documentService = documentService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get paginated list of documents
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResultDto<DocumentDto>>> GetDocuments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null)
    {
        var result = await _documentService.GetDocumentsAsync(page, pageSize, category, User);
        
        // Add total count to response header
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        
        return Ok(result);
    }
    
    /// <summary>
    /// Get a single document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(Guid id)
    {
        var document = await _documentService.GetDocumentByIdAsync(id, User);
        
        if (document == null)
        {
            return NotFound(new { message = "Document not found" });
        }
        
        return Ok(document);
    }
    
    /// <summary>
    /// Create a new document
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DocumentDto>> CreateDocument([FromBody] CreateDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var document = await _documentService.CreateDocumentAsync(dto, User);
        
        return CreatedAtAction(
            nameof(GetDocument),
            new { id = document.Id },
            document);
    }
    
    /// <summary>
    /// Update an existing document
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> UpdateDocument(Guid id, [FromBody] UpdateDocumentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var document = await _documentService.UpdateDocumentAsync(id, dto, User);
        
        if (document == null)
        {
            return NotFound(new { message = "Document not found" });
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// Soft delete a document
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "EditorOrAdmin")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        var result = await _documentService.DeleteDocumentAsync(id, User);
        
        if (!result)
        {
            return NotFound(new { message = "Document not found" });
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// Download a document file
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(Guid id)
    {
        // Get document with access control
        var document = await _documentService.GetDocumentByIdAsync(id, User);
        
        if (document == null)
        {
            return NotFound(new { message = "Document not found" });
        }
        
        // Get file path
        var filePath = await _fileStorageService.GetFilePathAsync(document.FilePath);
        
        // Check if file exists
        if (!await _fileStorageService.FileExistsAsync(filePath))
        {
            _logger.LogWarning("File not found for document {DocumentId} at path {FilePath}", id, filePath);
            return NotFound(new { message = "File not found" });
        }   
        
        // Determine file name for download
        var fileName = document.Title;
        var extension = Path.GetExtension(document.FilePath);
        if (!fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            fileName += extension;
        }
        
        // Stream file with proper content type
        return PhysicalFile(
            filePath,
            document.MimeType,
            fileName,
            enableRangeProcessing: true);
    }
}
