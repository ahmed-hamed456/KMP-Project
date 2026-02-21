using Microsoft.AspNetCore.Mvc;
using SearchService.Application.DTOs;
using SearchService.Application.Interfaces;
using System.Security.Claims;
using FluentValidation;

namespace SearchService.Api.Controllers;

[ApiController]
[Route("api/v1/search")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ISearchService searchService, ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Search for documents using full-text search
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SearchResponseDto>> Search([FromBody] SearchRequestDto request)
    {
        try
        {
            // Extract user context from claims
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Viewer";
            var userDepartmentIdStr = User.FindFirst("departmentId")?.Value;
            Guid? userDepartmentId = null;
            
            if (!string.IsNullOrEmpty(userDepartmentIdStr) && Guid.TryParse(userDepartmentIdStr, out var deptId))
            {
                userDepartmentId = deptId;
            }

            _logger.LogInformation("Search request received: Query={Query}, Page={Page}, PageSize={PageSize}, UserRole={UserRole}, UserDepartmentId={UserDepartmentId}",
                request.Query, request.Page, request.PageSize, userRole, userDepartmentId);

            var result = await _searchService.SearchAsync(request, userRole, userDepartmentId);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error in search request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing search request");
            return StatusCode(500, new { error = "An error occurred while processing your search" });
        }
    }

    /// <summary>
    /// Get search suggestions based on prefix
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<ActionResult<SuggestionsResponseDto>> GetSuggestions(
        [FromQuery] string prefix,
        [FromQuery] int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Length < 2)
            {
                return BadRequest(new { error = "Prefix must be at least 2 characters" });
            }

            if (limit < 1 || limit > 50)
            {
                return BadRequest(new { error = "Limit must be between 1 and 50" });
            }

            _logger.LogInformation("Suggestions request: Prefix={Prefix}, Limit={Limit}", prefix, limit);

            var result = await _searchService.GetSuggestionsAsync(prefix, limit);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions");
            return StatusCode(500, new { error = "An error occurred while getting suggestions" });
        }
    }

    /// <summary>
    /// Get facets (categories and departments) for filtering
    /// </summary>
    [HttpGet("facets")]
    public async Task<ActionResult<FacetsDto>> GetFacets()
    {
        try
        {
            _logger.LogInformation("Facets request received");

            var result = await _searchService.GetFacetsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting facets");
            return StatusCode(500, new { error = "An error occurred while getting facets" });
        }
    }
}
