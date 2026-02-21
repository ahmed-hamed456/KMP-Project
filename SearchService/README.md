# SearchService - Full-Text Search Microservice

A standalone search microservice for the Knowledge Management Platform (KMP) that provides full-text search capabilities with filtering, sorting, and autocomplete features.

---

## Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Running the Application](#running-the-application)
- [Search Implementation Approach](#search-implementation-approach)
- [Architecture](#architecture)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Assumptions](#assumptions)
- [Technology Stack](#technology-stack)

---

## Overview

SearchService is a .NET 10.0 microservice built using Clean Architecture principles. It maintains a synchronized read model of documents from the DocumentManagement API and provides fast full-text search using SQL Server Full-Text Search (FTS).

### Key Features

- **Full-Text Search** - Search across Title, Description, Tags with relevance ranking
- **Advanced Filtering** - Filter by categories, departments, date ranges
- **Autocomplete Suggestions** - Prefix-based suggestions for search queries
- **Search Facets** - Category and department aggregations with document counts
- **Automatic Sync** - Background service syncs documents from DocumentManagement API every 5 minutes
- **Input Validation** - FluentValidation for request validation
- **Search Highlighting** - Matched terms highlighted with `<em>` tags in results

---

## Prerequisites

Before running the application, ensure you have the following installed:

### Required
- **.NET 10.0 SDK** or later - [Download](https://dotnet.microsoft.com/download)
- **SQL Server LocalDB** (included with Visual Studio) or **SQL Server Express**
  - LocalDB: `Server=(localdb)\mssqllocaldb`
  - Express: `Server=localhost\SQLEXPRESS`

### Optional (for testing)
- **Visual Studio 2022** or **VS Code** with C# extension
- **Postman** or any REST client
- **SQL Server Management Studio (SSMS)** for database inspection

---

## Setup Instructions

### 1. Clone the Repository

```bash
cd D:\KMP-Project\SearchService
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Configure Connection String

The default configuration uses SQL Server LocalDB. If you're using a different SQL Server instance, update the connection string in `SearchService.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SearchServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

For SQL Server Express:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SearchServiceDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### 4. Apply Database Migrations

Navigate to the Infrastructure project and run migrations:

```bash
cd SearchService.Infrastructure
dotnet ef database update --startup-project ..\SearchService.Api
```

This will:
- Create the `SearchServiceDb` database
- Create the `SearchableDocuments` table
- Create a Full-Text Search catalog named `SearchCatalog`
- Create a Full-Text index on `Title`, `Description`, and `Tags` columns

### 5. Seed the Database

Run the seed data SQL script to populate the database with 25 sample documents:

```bash
cd ..
sqlcmd -S "(localdb)\MSSQLLocalDB" -d SearchServiceDb -i seed_data.sql
```

Or use SQL Server Management Studio to execute `seed_data.sql`.

**Seed Data Includes:**
- 25 documents across 5 departments (Finance, HR, Engineering, Marketing, Legal)
- 5 categories matching departments
- Varied tags and date ranges
- Similar content to test search relevance

### 6. Verify Full-Text Search Setup

Connect to your SQL Server database and verify FTS is enabled:

```sql
-- Check Full-Text Catalog
SELECT * FROM sys.fulltext_catalogs;

-- Check Full-Text Index
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    * 
FROM sys.fulltext_indexes;

-- Test Full-Text Search
SELECT TOP 5 Title, Description 
FROM SearchableDocuments 
WHERE CONTAINS((Title, Description, Tags), 'annual');
```

---

## Running the Application

### Start the API

```bash
cd SearchService.Api
dotnet run
```

The application will start on:
- **HTTP**: `http://localhost:5298`
- **HTTPS**: `https://localhost:7298`

### Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5298/swagger
```

This provides an interactive API documentation and testing interface.

### Stop the Application

Press `Ctrl+C` in the terminal where the application is running.

---

## Search Implementation Approach

### Implementation Choice: SQL Server Full-Text Search (FTS)

**Why SQL Server FTS?**

1. **Native Integration** - SQL Server FTS is built into SQL Server, requiring no additional infrastructure
2. **Performance** - Optimized for text search with inverted indexes
3. **Relevance Ranking** - Built-in ranking algorithm via `RANK` column
4. **Word Proximity** - Supports phrase searches and word proximity
5. **Simplicity** - No need for external search engines (Elasticsearch, Solr)
6. **Cost-Effective** - No additional licensing or hosting costs

### How It Works

**1. Full-Text Catalog & Index**

During migration, a Full-Text catalog and index are created:

```sql
-- Create catalog
CREATE FULLTEXT CATALOG SearchCatalog AS DEFAULT;

-- Create index on searchable columns
CREATE FULLTEXT INDEX ON SearchableDocuments(Title, Description, Tags)
KEY INDEX PK_SearchableDocuments
ON SearchCatalog
WITH CHANGE_TRACKING AUTO;
```

**2. Search Query Construction**

The search uses `CONTAINSTABLE` for full-text queries:

```csharp
var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
var ftsQuery = string.Join(" OR ", searchTerms.Select(t => $"\"{t}*\""));

// Example: "annual report" becomes: "annual*" OR "report*"
```

```sql
SELECT d.*, fts.[RANK] AS Score
FROM CONTAINSTABLE(SearchableDocuments, (Title, Description, Tags), @searchTerm) AS fts
INNER JOIN SearchableDocuments d ON fts.[KEY] = d.Id
WHERE d.IsDeleted = 0
ORDER BY fts.[RANK] DESC
```

**3. Wildcard Prefix Matching**

Each search term is suffixed with `*` to enable partial word matching:
- "annu" matches "annual", "anniversary"
- "rep" matches "report", "reporting"

**4. Relevance Scoring**

SQL Server FTS returns a `RANK` value (0-1000) indicating relevance:
- Higher rank = better match
- Based on term frequency, proximity, and field weights

**5. Search Term Highlighting**

After retrieving results, matched terms are highlighted:

```csharp
private string HighlightText(string text, string query)
{
    var queryTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var highlighted = text;

    foreach (var term in queryTerms)
    {
        var regex = new Regex(Regex.Escape(term), RegexOptions.IgnoreCase);
        highlighted = regex.Replace(highlighted, m => $"<em>{m.Value}</em>");
    }

    return highlighted;
}
```

**Result:**
```json
{
  "title": "Annual Budget Report",
  "highlightedTitle": "<em>Annual</em> Budget <em>Report</em>"
}
```

### Alternative Approaches Considered

| Approach | Pros | Cons | Decision |
|----------|------|------|----------|
| **LIKE Queries** | Simple, no FTS setup | Poor performance, no ranking | ❌ Not scalable |
| **EF Core LINQ** | Type-safe, LINQ syntax | Limited FTS capabilities | ❌ No relevance scoring |
| **Elasticsearch** | Powerful, scalable | Additional infrastructure, complexity | ❌ Overkill for requirements |
| **SQL Server FTS** | Native, performant, ranking | Requires FTS-enabled SQL Server | ✅ **Selected** |

---

## Architecture

The project follows **Clean Architecture** (Onion Architecture) with clear separation of concerns:

### Project Structure

```
SearchService/
├── SearchService.Domain/              # Core business entities and interfaces
│   ├── Entities/
│   │   ├── BaseEntity.cs             # Base entity (Id, CreatedAt, UpdatedAt)
│   │   └── SearchableDocument.cs     # Read model for searchable documents
│   └── Interfaces/
│       ├── ISearchRepository.cs       # Repository abstraction
│       └── IDocumentManagementClient.cs # External API client abstraction
│
├── SearchService.Application/         # Business logic and use cases
│   ├── DTOs/
│   │   ├── SearchRequestDto.cs       # Search request model
│   │   ├── SearchResponseDto.cs      # Search response model
│   │   ├── SearchResultDto.cs        # Individual result model
│   │   ├── SearchFiltersDto.cs       # Filter criteria
│   │   ├── FacetsDto.cs              # Facet aggregations
│   │   └── SuggestionsResponseDto.cs # Autocomplete suggestions
│   ├── Interfaces/
│   │   ├── ISearchService.cs         # Search service interface
│   │   └── IDocumentSyncService.cs   # Sync service interface
│   ├── Services/
│   │   ├── SearchService.cs          # Search business logic
│   │   └── DocumentSyncService.cs    # Sync business logic
│   └── Validators/
│       └── SearchRequestValidator.cs  # FluentValidation rules
│
├── SearchService.Infrastructure/      # Data access and external services
│   ├── Data/
│   │   └── SearchDbContext.cs        # EF Core DbContext
│   ├── Repositories/
│   │   └── SearchRepository.cs       # Search data access with FTS
│   ├── HttpClients/
│   │   └── DocumentManagementClient.cs # HTTP client for DocumentManagement API
│   ├── BackgroundServices/
│   │   └── DocumentSyncBackgroundService.cs # Periodic sync (every 5 min)
│   └── Migrations/
│       ├── InitialCreate.cs          # Database schema
│       └── SeedData.cs               # Sample data
│
└── SearchService.Api/                 # REST API and presentation layer
    ├── Controllers/
    │   └── SearchController.cs       # API endpoints
    ├── Middleware/
    │   └── MockAuthenticationMiddleware.cs # Header-based auth
    ├── Program.cs                    # Application startup
    ├── appsettings.json              # Configuration
    └── SearchTests.http              # API test file
```

### Dependency Flow

```
Api → Application → Domain
  ↓                    ↑
Infrastructure ────────┘
```

- **Domain** has no dependencies (pure business logic)
- **Application** depends on Domain interfaces
- **Infrastructure** implements Domain interfaces
- **Api** orchestrates everything via dependency injection

---

## API Endpoints

### 1. Full-Text Search

**POST** `/api/v1/search`

Search for documents with filtering, sorting, and pagination.

**Request Headers:**
```http
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Role: Admin
X-Department-Id: 123e4567-e89b-12d3-a456-426614174001
Content-Type: application/json
```

**Request Body:**
```json
{
  "query": "annual report",
  "filters": {
    "categories": ["Finance", "HR"],
    "departmentIds": ["D1111111-1111-1111-1111-111111111111"],
    "fromDate": "2025-11-01T00:00:00Z",
    "toDate": "2026-02-28T23:59:59Z"
  },
  "page": 1,
  "pageSize": 10,
  "sortBy": "relevance",
  "ascending": false
}
```

**Query Parameters:**
- `query` (required): Search query string (2-200 characters)
- `filters` (optional): Filter criteria
  - `categories`: Array of category names
  - `departmentIds`: Array of department GUIDs
  - `fromDate`: Start date (ISO 8601)
  - `toDate`: End date (ISO 8601)
- `page` (default: 1): Page number (≥ 1)
- `pageSize` (default: 10): Results per page (1-100)
- `sortBy` (default: relevance): Sort field (relevance, createdAt, updatedAt, title)
- `ascending` (default: false): Sort direction

**Response (200 OK):**
```json
{
  "results": [
    {
      "id": "11111111-1111-1111-1111-000000000002",
      "title": "Annual Budget Planning 2026",
      "highlightedTitle": "<em>Annual</em> Budget Planning 2026",
      "description": "Strategic budget allocation and financial planning for fiscal year 2026",
      "highlightedDescription": "Strategic budget allocation and financial planning for fiscal year 2026",
      "category": "Finance",
      "tags": ["budget", "planning", "annual", "strategy", "2026"],
      "departmentId": "d1111111-1111-1111-1111-111111111111",
      "departmentName": "Finance Department",
      "createdAt": "2025-12-07T23:26:18.37",
      "updatedAt": "2025-12-12T23:26:18.37",
      "score": 64
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1,
  "facets": null
}
```

**Validation Errors (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Query": ["Search query must be at least 2 characters"],
    "PageSize": ["PageSize must be between 1 and 100"]
  }
}
```

---

### 2. Autocomplete Suggestions

**GET** `/api/v1/search/suggestions?prefix={prefix}&limit={limit}`

Get autocomplete suggestions based on document titles.

**Request Headers:**
```http
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Role: Admin
X-Department-Id: 123e4567-e89b-12d3-a456-426614174001
```

**Query Parameters:**
- `prefix` (required): Search prefix (minimum 2 characters)
- `limit` (default: 10): Maximum suggestions to return (1-50)

**Example Request:**
```http
GET /api/v1/search/suggestions?prefix=ann&limit=10
```

**Response (200 OK):**
```json
{
  "suggestions": [
    "Annual Budget Planning 2026",
    "API Development Standards"
  ]
}
```

**Error (400 Bad Request):**
```json
{
  "error": "Prefix must be at least 2 characters"
}
```

---

### 3. Search Facets

**GET** `/api/v1/search/facets`

Get category and department facets for building filter UIs.

**Request Headers:**
```http
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Role: Admin
X-Department-Id: 123e4567-e89b-12d3-a456-426614174001
```

**Response (200 OK):**
```json
{
  "categories": [
    { "category": "Engineering", "count": 5 },
    { "category": "Finance", "count": 5 },
    { "category": "HR", "count": 5 },
    { "category": "Legal", "count": 5 },
    { "category": "Marketing", "count": 5 }
  ],
  "departments": [
    { 
      "departmentId": "d1111111-1111-1111-1111-111111111111", 
      "departmentName": "Finance Department", 
      "count": 5 
    },
    { 
      "departmentId": "d2222222-2222-2222-2222-222222222222", 
      "departmentName": "HR Department", 
      "count": 5 
    },
    { 
      "departmentId": "d3333333-3333-3333-3333-333333333333", 
      "departmentName": "Engineering Department", 
      "count": 5 
    },
    { 
      "departmentId": "d4444444-4444-4444-4444-444444444444", 
      "departmentName": "Marketing Department", 
      "count": 5 
    },
    { 
      "departmentId": "d5555555-5555-5555-5555-555555555555", 
      "departmentName": "Legal Department", 
      "count": 5 
    }
  ]
}
```

---

## Testing

### Using the Provided HTTP File

Open `SearchTests.http` in Visual Studio Code with the REST Client extension or Visual Studio 2022.

The file contains 9 test scenarios:

1. **Basic Search** - Simple keyword search
2. **Search with Filters** - Category, department, and date filters
3. **Sort by Title** - Alphabetical sorting
4. **Get Suggestions** - Autocomplete test
5. **Get Facets** - Facet aggregations
6. **Invalid Query** - Validation error (query too short)
7. **Invalid Page Size** - Validation error (page size too large)
8. **Invalid Prefix** - Suggestions with too short prefix
9. **Health Check** - (optional endpoint)

### Using Postman

Import the requests from `SearchTests.http` or manually create requests:

**Example 1: Basic Search**
```http
POST http://localhost:5298/api/v1/search
Content-Type: application/json
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Role: Admin
X-Department-Id: 123e4567-e89b-12d3-a456-426614174001

{
  "query": "report",
  "page": 1,
  "pageSize": 10
}
```

**Example 2: Search with Filters**
```http
POST http://localhost:5298/api/v1/search
Content-Type: application/json
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Role: Admin
X-Department-Id: 123e4567-e89b-12d3-a456-426614174001

{
  "query": "annual",
  "filters": {
    "categories": ["Finance"],
    "fromDate": "2025-11-01T00:00:00Z",
    "toDate": "2026-02-28T23:59:59Z"
  },
  "page": 1,
  "pageSize": 10,
  "sortBy": "relevance"
}
```

### Sample Department IDs (from seed data)

- Finance: `D1111111-1111-1111-1111-111111111111`
- HR: `D2222222-2222-2222-2222-222222222222`
- Engineering: `D3333333-3333-3333-3333-333333333333`
- Marketing: `D4444444-4444-4444-4444-444444444444`
- Legal: `D5555555-5555-5555-5555-555555555555`

---

## Assumptions

### 1. Data Synchronization

**Assumption:** The DocumentManagement API is running on `http://localhost:5297` and is accessible.

**Implication:** 
- Background sync service polls every 5 minutes
- If DocumentManagement API is not running, sync will fail silently (logged as warnings)
- Search will work with locally cached data

### 2. Authentication

**Assumption:** Mock authentication via HTTP headers is sufficient for development/testing.

**Implication:**
- No JWT or OAuth implementation
- Headers are trusted without validation
- Production would require proper authentication middleware

### 3. Search Scope

**Assumption:** Search only covers document metadata (Title, Description, Tags, Category), not file content.

**Implication:**
- Fast search performance
- No need for file parsing
- Document content is stored separately in DocumentManagement API

### 4. Role-Based Access

**Assumption:** 
- **Viewer** role: Can only search documents in their own department
- **Admin/Manager** roles: Can search all documents

**Implication:**
- Department-based filtering applied automatically for Viewers
- Admins/Managers have unrestricted search access

### 5. Soft Deletes

**Assumption:** Documents are soft-deleted (IsDeleted flag) rather than hard-deleted.

**Implication:**
- Deleted documents remain in database but excluded from search
- Allows for potential "undo" or audit trail
- Periodic cleanup may be needed

### 6. SQL Server Full-Text Search

**Assumption:** SQL Server instance supports Full-Text Search.

**Implication:**
- SQL Server LocalDB works for development (FTS supported since SQL Server 2016)
- SQL Server Express Free Edition also supports FTS
- Azure SQL Database supports FTS (not included in Basic tier)

### 7. Performance

**Assumption:** Dataset size is moderate (thousands to tens of thousands of documents).

**Implication:**
- FTS indexing handles this scale efficiently
- No need for distributed search (Elasticsearch)
- Pagination keeps response sizes manageable

### 8. Date Filtering

**Assumption:** Date filters apply to document `CreatedAt` timestamp.

**Implication:**
- Cannot filter by `UpdatedAt` directly (would require separate filter parameter)
- Date ranges are inclusive (fromDate ≤ createdAt ≤ toDate)

### 9. Tag Storage

**Assumption:** Tags are stored as comma-separated strings rather than a separate Tags table.

**Implication:**
- Simpler schema, easier FTS indexing
- Tag management happens in DocumentManagement API
- Duplicate tags may exist across documents

### 10. Error Handling

**Assumption:** Standard HTTP status codes and problem details format.

**Implication:**
- 400 for validation errors
- 500 for server errors
- Detailed error messages in development, generic in production

---

## Technology Stack

### Frameworks & Libraries

- **.NET 10.0** - Latest .NET runtime
- **ASP.NET Core 10.0** - Web API framework
- **Entity Framework Core 10.0.3** - ORM for database access
- **FluentValidation 11.11.0** - Request validation
- **FluentValidation.AspNetCore 11.3.1** - ASP.NET Core integration

### Database

- **SQL Server LocalDB** - Development database (mssqllocaldb)
- **SQL Server Full-Text Search** - FTS catalog and indexes

### Tools & Utilities

- **Swagger/OpenAPI** - API documentation (Swashbuckle.AspNetCore 10.1.4)
- **HttpClient** - Service-to-service communication
- **IHostedService** - Background sync worker
- **PeriodicTimer** - Scheduled background tasks

### Design Patterns

- **Clean Architecture** - Separation of concerns across layers
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Built-in ASP.NET Core DI
- **CQRS** - Separate read model for search
- **Middleware Pattern** - Authentication and exception handling

---

## Configuration

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SearchServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "DocumentManagementApi": {
    "BaseUrl": "http://localhost:5297"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### Environment Variables (Optional)

Override settings via environment variables:

```bash
export ConnectionStrings__DefaultConnection="Server=myserver;Database=SearchServiceDb;..."
export DocumentManagementApi__BaseUrl="http://documentapi:5297"
```

---

## Troubleshooting

### Issue: Migration fails with "Full-Text Search is not installed"

**Solution:** Use SQL Server Express or full SQL Server instead of LocalDB (older versions).

### Issue: No search results returned

**Check:**
1. Verify seed data exists: `SELECT COUNT(*) FROM SearchableDocuments WHERE IsDeleted = 0`
2. Check FTS catalog: `SELECT * FROM sys.fulltext_catalogs`
3. Check FTS index: `SELECT * FROM sys.fulltext_indexes`
4. Test FTS directly: `SELECT * FROM SearchableDocuments WHERE CONTAINS(Title, 'annual')`

### Issue: Background sync not working

**Check:**
1. DocumentManagement API is running on port 5297
2. Check logs for sync errors: Look for "Document sync completed" or error messages
3. Verify HTTP client configuration in `Program.cs`

### Issue: Validation errors not showing

**Check:**
1. FluentValidation packages installed
2. `AddFluentValidationAutoValidation()` called in `Program.cs`
3. Validators registered in DI container

### Issue: Port already in use

**Solution:**
```bash
# Find process using port 5298
netstat -ano | findstr :5298

# Kill the process
taskkill /PID <process_id> /F
```

---

## Future Enhancements

- [ ] Add Docker support with docker-compose
- [ ] Implement real JWT authentication
- [ ] Add Redis caching for frequent searches
- [ ] Implement search analytics and query logging
- [ ] Add search query suggestions based on popular searches
- [ ] Implement synonym support (e.g., "doc" → "document")
- [ ] Add more advanced FTS features (phrase search, proximity)
- [ ] Implement search result pagination cursors
- [ ] Add health check endpoint
- [ ] Implement distributed tracing (OpenTelemetry)

---

## License

This project is part of the Knowledge Management Platform (KMP) and is for assignment/demonstration purposes.

---

## Contact

For questions or issues, please contact the development team.
    ├── appsettings.json              # Configuration
    └── SearchTests.http              # API test file
```

### Dependency Flow

```
Api → Application → Domain
  ↓                    ↑
Infrastructure ────────┘
```

- **Domain** has no dependencies (pure business logic)
- **Application** depends on Domain interfaces
- **Infrastructure** implements Domain interfaces
- **Api** orchestrates everything via dependency injection
```

## Validation Rules

### Search Request
- `query`: Required, 2-200 characters
- `page`: Must be > 0
- `pageSize`: Must be 1-100
- `sortBy`: Must be one of: relevance, createdAt, updatedAt, title
- `fromDate` must be <= `toDate` (if both specified)

### Suggestions
- `prefix`: Required, minimum 2 characters
- `limit`: Must be 1-50

## Data Synchronization

The `DocumentSyncBackgroundService` runs every 5 minutes to:
1. Fetch all documents from DocumentManagement API (`http://localhost:5297/api/documents`)
2. Upsert documents into the local SearchableDocuments table
3. Delete documents marked as `IsDeleted`
4. Update `LastSyncedAt` timestamp

## Database Schema

### SearchableDocuments Table
- `Id` (uniqueidentifier, PK)
- `Title` (nvarchar(500), required)
- `Description` (nvarchar(2000), nullable)
- `Category` (nvarchar(100), required)
- `Tags` (nvarchar(1000)) - comma-separated
- `DepartmentId` (uniqueidentifier)
- `DepartmentName` (nvarchar(200))
- `IsDeleted` (bit)
- `LastSyncedAt` (datetime2)
- `CreatedAt` (datetime2)
- `UpdatedAt` (datetime2, nullable)

### Indexes
- IX_SearchableDocuments_Category
- IX_SearchableDocuments_DepartmentId
- IX_SearchableDocuments_IsDeleted
- IX_SearchableDocuments_CreatedAt
- Full-Text Index on (Title, Description, Tags, Category)

## Testing

Use the included `SearchTests.http` file with VS Code REST Client extension or similar tools.

Key test scenarios:
1. Basic search
2. Search with filters
3. Search with different sort options
4. Suggestions
5. Facets
6. Validation errors

## Project Structure

```
SearchService/
├── SearchService.sln
├── SearchService.Domain/
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   └── SearchableDocument.cs
│   └── Interfaces/
│       ├── ISearchRepository.cs
│       └── IDocumentManagementClient.cs
├── SearchService.Application/
│   ├── DTOs/
│   │   ├── SearchRequestDto.cs
│   │   ├── SearchResponseDto.cs
│   │   ├── SearchResultDto.cs
│   │   ├── SearchFiltersDto.cs
│   │   ├── FacetsDto.cs
│   │   └── SuggestionsResponseDto.cs
│   ├── Interfaces/
│   │   ├── ISearchService.cs
│   │   └── IDocumentSyncService.cs
│   ├── Services/
│   │   ├── SearchService.cs
│   │   └── DocumentSyncService.cs
│   └── Validators/
│       └── SearchRequestValidator.cs
├── SearchService.Infrastructure/
│   ├── Data/
│   │   └── SearchDbContext.cs
│   ├── Repositories/
│   │   └── SearchRepository.cs
│   ├── HttpClients/
│   │   └── DocumentManagementClient.cs
│   ├── BackgroundServices/
│   │   └── DocumentSyncBackgroundService.cs
│   └── Migrations/
│       └── 20260220_InitialCreate.cs
└── SearchService.Api/
    ├── Controllers/
    │   └── SearchController.cs
    ├── Middleware/
    │   └── MockAuthenticationMiddleware.cs
    ├── Program.cs
    ├── appsettings.json
    └── appsettings.Development.json
```

## Development Notes

- The microservice is completely independent of DocumentManagement API's database
- Communication happens only via HTTP/REST
- Can be deployed separately with its own scaling strategy
- Uses mock authentication for development (should be replaced with real auth in production)

## Future Enhancements

- Implement actual JWT authentication
- Add health check endpoint
- Add metrics and monitoring
- Implement advanced FTS features (proximity search, weighted columns)
- Add caching layer (Redis)
- Add search analytics
- Implement incremental sync (only fetch changed documents)
