# KMP Project Structure

Updated: February 20, 2026

## Overview

The KMP (Knowledge Management Platform) project is now organized into two main microservices:

1. **KMP-Core**: The core document management system
2. **SearchService**: The standalone search microservice

## Directory Structure

```
d:\KMP-Project\
│
├── KMP-Core/                                    # Core Document Management System
│   ├── DocumentManagement.Api/                 # Web API Layer
│   ├── DocumentManagement.Application/         # Business Logic Layer
│   ├── DocumentManagement.Domain/              # Domain Entities & Interfaces
│   ├── DocumentManagement.Infrastructure/      # Data Access & External Services
│   ├── DocumentManagement.sln                  # Solution File
│   ├── DocumentTests.http                      # API Test Requests
│   └── README.md                               # Documentation
│
├── SearchService/                              # Search Microservice
│   ├── SearchService.Api/                      # Web API Layer
│   ├── SearchService.Application/              # Business Logic Layer
│   ├── SearchService.Domain/                   # Domain Entities & Interfaces
│   ├── SearchService.Infrastructure/           # Data Access & HTTP Clients
│   ├── SearchService.sln                       # Solution File
│   ├── SearchTests.http                        # API Test Requests
│   ├── README.md                               # Documentation
│   ├── QUICKSTART.md                           # Quick Start Guide
│   └── IMPLEMENTATION_SUMMARY.md               # Implementation Details
│
├── .git/                                       # Git Repository
├── .gitignore                                  # Git Ignore Rules
└── .vs/                                        # Visual Studio Settings

```

## Microservice Details

### KMP-Core (DocumentManagement)

**Purpose**: Core document management system with CRUD operations, role-based access control, and audit trails.

**Port**: 5297 (HTTP), 7297 (HTTPS)

**Database**: DocumentManagementDb (SQL Server LocalDB)

**Key Features**:
- Document CRUD operations
- Role-based access control (Admin, Manager, Employee)
- Department-based document ownership
- Soft delete and audit trail
- Data seeding for development

**To Run**:
```bash
cd d:\KMP-Project\KMP-Core
dotnet build
cd DocumentManagement.Api
dotnet run
```

**Access**: http://localhost:5297/swagger

---

### SearchService

**Purpose**: Standalone search microservice providing full-text search capabilities across document metadata.

**Port**: 5298 (HTTP), 7298 (HTTPS)

**Database**: SearchServiceDb (SQL Server LocalDB) - **Separate database**

**Key Features**:
- Full-text search with SQL Server FTS
- Search term highlighting
- Autocomplete suggestions
- Search facets (categories, departments)
- Automatic data synchronization from DocumentManagement API (every 5 minutes)
- HTTP-based service-to-service communication

**To Run**:
```bash
cd d:\KMP-Project\SearchService
dotnet build
cd SearchService.Api
dotnet run
```

**Access**: http://localhost:5298/swagger

---

## Service Communication

The SearchService microservice communicates with KMP-Core via HTTP:

```
SearchService (Port 5298)
    │
    │ HTTP GET /api/documents
    │ (Every 5 minutes via background service)
    ▼
KMP-Core DocumentManagement.Api (Port 5297)
```

**Configuration**: The SearchService URL for DocumentManagement API is configured in:
- `SearchService.Api/appsettings.json`
- `SearchService.Api/appsettings.Development.json`

```json
{
  "DocumentManagementApi": {
    "BaseUrl": "http://localhost:5297"
  }
}
```

## Development Workflow

### Starting Both Services

1. **Start KMP-Core** (Terminal 1):
   ```bash
   cd d:\KMP-Project\KMP-Core\DocumentManagement.Api
   dotnet run
   ```
   Access at: http://localhost:5297

2. **Start SearchService** (Terminal 2):
   ```bash
   cd d:\KMP-Project\SearchService\SearchService.Api
   dotnet run
   ```
   Access at: http://localhost:5298

### Running Migrations

**KMP-Core**:
```bash
cd d:\KMP-Project\KMP-Core\DocumentManagement.Infrastructure
dotnet ef database update --startup-project ..\DocumentManagement.Api
```

**SearchService**:
```bash
cd d:\KMP-Project\SearchService\SearchService.Infrastructure
dotnet ef database update --startup-project ..\SearchService.Api
```

## Architecture

Both microservices follow **Clean Architecture** principles:

```
Api Layer           → Controllers, Middleware, Configuration
    ↓
Infrastructure      → Repositories, DbContext, HTTP Clients
    ↓
Application Layer   → Services, DTOs, Validators
    ↓
Domain Layer        → Entities, Interfaces
```

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server (LocalDB for development)
- **ORM**: Entity Framework Core 10.x
- **Validation**: FluentValidation
- **Authentication**: Mock header-based (for development)
- **Documentation**: Swagger/OpenAPI

## Authentication

Both services use the same mock authentication pattern for development:

**Headers**:
- `X-User-Id`: User identifier (GUID)
- `X-User-Role`: Admin | Manager | Employee
- `X-Department-Id`: Department identifier (GUID)

**Example**:
```http
GET /api/documents HTTP/1.1
Host: localhost:5297
X-User-Id: 123e4567-e89b-12d3-a456-426614174000
X-User-Role: Admin
X-Department-Id: 123e4567-e89b-12d3-a456-426614174001
```

## Testing

Each microservice has its own HTTP test file:

- **KMP-Core**: `d:\KMP-Project\KMP-Core\DocumentTests.http`
- **SearchService**: `d:\KMP-Project\SearchService\SearchTests.http`

Use VS Code with the REST Client extension to execute these tests.

## Notes

- Both microservices are completely independent
- Each has its own database
- Communication is via HTTP only (no direct database access between services)
- SearchService maintains a read model synced from KMP-Core
- Both can be deployed, scaled, and updated independently

## Migration Notes

**Previous Structure**: All DocumentManagement projects were in the root `d:\KMP-Project\` directory.

**Current Structure**: All DocumentManagement projects moved to `d:\KMP-Project\KMP-Core\` for better organization.

**Impact**: None - all relative paths in solution files remain correct. The build and run commands now need to be executed from the KMP-Core directory.

---

**For detailed setup instructions, see:**
- KMP-Core: `d:\KMP-Project\KMP-Core\README.md`
- SearchService: `d:\KMP-Project\SearchService\QUICKSTART.md`
