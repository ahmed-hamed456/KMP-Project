# Document Management API

A RESTful API for managing documents with role-based access control, built using Clean Architecture principles with ASP.NET Core.

## 📋 Prerequisites

Before running this project, ensure you have the following installed:

- **.NET SDK 8.0 or later** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server** (Express, Developer, or full version) - [Download here](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **Code Editor**: Visual Studio 2022, VS Code, or JetBrains Rider
- **Optional Tools**:
  - [Postman](https://www.postman.com/downloads/) for API testing
  - [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension for VS Code
  - [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) for database management

## 🏗️ Architecture

This project follows **Clean Architecture** principles with the following layers:

```
DocumentManagement.sln
├── DocumentManagement.Domain          # Core entities, interfaces, exceptions
├── DocumentManagement.Application     # Business logic, DTOs, services, validators
├── DocumentManagement.Infrastructure  # Data access, repositories, file storage
└── DocumentManagement.Api             # Controllers, middleware, authentication
```

## 🚀 Setup Instructions

### 1. Clone or Extract the Project

```bash
cd d:\KMP-Project
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Configure Database Connection

The application uses SQL Server. The default connection string in `appsettings.json` is:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=DocumentManagementDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

**Note**: `Server=.` means local SQL Server instance. Update this if using a different server.

### 4. Apply Database Migrations

Navigate to the API project and run:

```bash
cd DocumentManagement.Api
dotnet ef database update
```

This will:
- Create the `DocumentManagementDb` database
- Create `Departments` and `Documents` tables with proper relationships
- Seed 3 departments: IT, Finance, HR
- Seed 5 sample documents with different file types
- Generate sample files in `AppData/Documents` folder

### 5. Verify Setup

After migration, the following sample files will be created in `DocumentManagement.Api/bin/Debug/net10.0/AppData/Documents/`:

| File Name | Type | Size | Department |
|-----------|------|------|------------|
| q4-financial-report.txt | text/plain | ~182 bytes | Finance |
| it-infrastructure-plan.pdf | application/pdf | ~951 bytes | IT |
| employee-handbook-2025.json | application/json | ~683 bytes | HR |
| q1-budget-analysis.xml | application/xml | ~605 bytes | Finance |
| network-security-guidelines.csv | text/csv | ~370 bytes | IT |

## ▶️ How to Run the Application

### Start the API Server

From the API project directory:

```bash
cd DocumentManagement.Api
dotnet run
```

Or from the solution root:

```bash
dotnet run --project DocumentManagement.Api
```

The API will start and listen on:
- **HTTP**: `http://localhost:5297`
- **HTTPS**: `https://localhost:7000` (if configured)

You should see output similar to:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5297
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

### Build the Solution

To build without running:

```bash
dotnet build
```

### Clean Build Artifacts

```bash
dotnet clean
```

## 🧪 How to Test the API

### Authentication Headers

This API uses **mock header-based authentication** for development. All requests require these headers:

| Header | Description | Example Value |
|--------|-------------|---------------|
| `X-User-Id` | User identifier (GUID) | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` |
| `X-User-Role` | User role | `Admin`, `Editor`, or `Viewer` |
| `X-Department-Id` | Department identifier (GUID) | `11111111-1111-1111-1111-111111111111` |

### Pre-configured Test Users

**Admin User (IT Department):**
```
X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
X-User-Role: Admin
X-Department-Id: 11111111-1111-1111-1111-111111111111
```

**Editor User (Finance Department):**
```
X-User-Id: bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb
X-User-Role: Editor
X-Department-Id: 22222222-2222-2222-2222-222222222222
```

**Viewer User (HR Department):**
```
X-User-Id: cccccccc-cccc-cccc-cccc-cccccccccccc
X-User-Role: Viewer
X-Department-Id: 33333333-3333-3333-3333-333333333333
```

### Testing Methods

#### Option 1: Using Postman

1. **List All Documents**
   ```
   GET http://localhost:5297/api/documents?page=1&pageSize=10
   Headers:
     X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
     X-User-Role: Admin
     X-Department-Id: 11111111-1111-1111-1111-111111111111
   ```

2. **Get Document by ID**
   ```
   GET http://localhost:5297/api/documents/d1111111-1111-1111-1111-111111111111
   Headers: (same as above)
   ```

3. **Download Document**
   ```
   GET http://localhost:5297/api/documents/d2222222-2222-2222-2222-222222222222/download
   Headers: (same as above)
   ```

4. **Create Document**
   ```
   POST http://localhost:5297/api/documents
   Headers: (same as above)
   Content-Type: application/json
   
   Body:
   {
     "title": "New Document",
     "description": "Test document",
     "category": "Finance",
     "tags": ["test", "sample"],
     "departmentId": "22222222-2222-2222-2222-222222222222"
   }
   ```

5. **Update Document**
   ```
   PUT http://localhost:5297/api/documents/d1111111-1111-1111-1111-111111111111
   Headers: (same as above)
   Content-Type: application/json
   
   Body:
   {
     "title": "Updated Title",
     "description": "Updated description",
     "category": "Finance",
     "tags": ["updated", "test"]
   }
   ```

6. **Delete Document (Soft Delete)**
   ```
   DELETE http://localhost:5297/api/documents/d1111111-1111-1111-1111-111111111111
   Headers: (same as above)
   ```

#### Option 2: Using cURL

```bash
# List documents
curl -X GET "http://localhost:5297/api/documents?page=1&pageSize=10" \
  -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "X-User-Role: Admin" \
  -H "X-Department-Id: 11111111-1111-1111-1111-111111111111"

# Download a PDF document
curl -X GET "http://localhost:5297/api/documents/d2222222-2222-2222-2222-222222222222/download" \
  -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "X-User-Role: Admin" \
  -H "X-Department-Id: 11111111-1111-1111-1111-111111111111" \
  --output it-infrastructure-plan.pdf

# Create a document
curl -X POST "http://localhost:5297/api/documents" \
  -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "X-User-Role: Admin" \
  -H "X-Department-Id: 11111111-1111-1111-1111-111111111111" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Document","description":"Testing API","category":"Technical","tags":["api","test"],"departmentId":"11111111-1111-1111-1111-111111111111"}'
```

#### Option 3: Using VS Code REST Client

Open `DocumentTests.http` in VS Code and click "Send Request" above each request.

### Sample Document IDs

Use these pre-seeded document IDs for testing:

| ID | Title | Type | Department |
|----|-------|------|------------|
| `d1111111-1111-1111-1111-111111111111` | Q4 Financial Report | TXT | Finance |
| `d2222222-2222-2222-2222-222222222222` | IT Infrastructure Plan 2025 | PDF | IT |
| `d3333333-3333-3333-3333-333333333333` | Employee Handbook 2025 | JSON | HR |
| `d4444444-4444-4444-4444-444444444444` | Budget Analysis Q1 | XML | Finance |
| `d5555555-5555-5555-5555-555555555555` | Network Security Guidelines | CSV | IT |

## 🔐 Role-Based Access Control

### Permissions Matrix

| Role | List Documents | View Document | Create | Update | Delete | Download |
|------|----------------|---------------|--------|--------|--------|----------|
| **Admin** | All documents | All | ✓ | ✓ | ✓ | All |
| **Editor** | All documents | All | ✓ | ✓ | ✗ | All |
| **Viewer** | Own department only | Own department only | ✗ | ✗ | ✗ | Own department only |

### Access Rules

- **Viewers** can only access documents from their own department
- **Editors** can create and update but cannot delete
- **Admins** have full access to all operations and departments
- Soft-deleted documents are hidden from all users except Admins

## 📚 API Endpoints

### Documents

- `GET /api/documents` - List documents (paginated, filtered by access level)
- `GET /api/documents/{id}` - Get document by ID
- `POST /api/documents` - Create new document
- `PUT /api/documents/{id}` - Update document
- `DELETE /api/documents/{id}` - Soft delete document
- `GET /api/documents/{id}/download` - Download document file

### Query Parameters

- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20)
- `category` - Filter by category
- `departmentId` - Filter by department
- `tag` - Filter by tag

## 🔧 Technical Details

### Technologies Used

- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core 8.0** - ORM for data access
- **SQL Server** - Database
- **FluentValidation** - Input validation
- **Swagger/OpenAPI** - API documentation (if enabled)

### Design Patterns

- **Clean Architecture** - Separation of concerns across layers
- **Repository Pattern** - Data access abstraction
- **Dependency Injection** - Loose coupling and testability
- **CQRS-inspired** - Separation of read/write operations through DTOs

### Key Features

- ✅ Role-based access control (Admin, Editor, Viewer)
- ✅ Department-based data isolation
- ✅ File upload and download with streaming
- ✅ Soft delete with audit tracking
- ✅ Pagination and filtering
- ✅ Input validation with FluentValidation
- ✅ Global exception handling
- ✅ Multiple file type support (PDF, JSON, XML, CSV, TXT)

## 🧩 Assumptions Made

1. **Authentication**: Mock header-based authentication is used for development/testing. In production, this should be replaced with JWT tokens or another secure authentication mechanism.

2. **File Storage**: Files are stored locally in the `AppData/Documents` folder. For production, consider using cloud storage (Azure Blob Storage, AWS S3, etc.).

3. **Database**: SQL Server is used with Windows Authentication (`Trusted_Connection=true`). Adjust connection string for SQL Authentication if needed.

4. **User Management**: User data is passed via headers. No actual user registration or management system is implemented.

5. **Department Assignment**: Users belong to a single department. Documents belong to a single department.

6. **File Upload**: The current implementation focuses on metadata management. File upload functionality can be added by extending the `POST /api/documents` endpoint with multipart/form-data support.

7. **Soft Delete**: Deleted documents are marked with `IsDeleted = true` rather than being physically removed from the database.

8. **Pagination**: Default page size is 20 items. Maximum page size is not enforced but should be added for production.

9. **File Types**: The seeder creates sample files with various MIME types (PDF, JSON, XML, CSV, TXT) to demonstrate multi-format support.

10. **Error Handling**: Global exception handling middleware returns JSON error responses. All exceptions are logged.

11. **CORS**: Not configured by default. Add CORS policy if the API will be consumed by a frontend application from a different origin.

12. **HTTPS**: The application supports HTTPS but HTTP is used by default on port 5297 for easier testing.

## 🛠️ Troubleshooting

### Port Already in Use

If you see an error about port 5297 being in use:

```bash
# Windows PowerShell
$port = 5297
$processId = (Get-NetTCPConnection -LocalPort $port).OwningProcess
Stop-Process -Id $processId -Force
```

### Database Connection Issues

- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure you have permissions to create databases
- Try using SQL Server Authentication instead of Windows Authentication

### Migration Errors

```bash
# Drop and recreate database
dotnet ef database drop --project DocumentManagement.Api --force
dotnet ef database update --project DocumentManagement.Api
```

### Files Not Created

Files are created when the application starts and seeds the database. If files are missing:

1. Check `DocumentManagement.Api/bin/Debug/net10.0/AppData/Documents/`
2. Restart the application to trigger seeding
3. Check logs for any seeding errors

## 📝 License

This project is for educational/evaluation purposes.

## 👤 Author

Backend Developer Assignment - Document Management System
- Create the `DocumentManagementDb` database
- Create `Departments` and `Documents` tables
- Seed 3 departments (IT, Finance, HR)
- Seed 5 sample documents
- Create sample files in `AppData/Documents` folder

### 5. Verify Sample Files Exist

Ensure these files exist in `DocumentManagement.Api/AppData/Documents/`:
- q4-financial-report.pdf
- it-infrastructure-plan.pdf
- employee-handbook-2025.pdf
- q1-budget-analysis.xlsx
- network-security-guidelines.docx

## ▶️ Running the Application

### Start the API

```bash
cd DocumentManagement.Api
dotnet run
```

The API will start on:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`

### Build the Solution

```bash
dotnet build
```

## 🧪 Testing the API

### Option 1: Using the .http File

Open `DocumentTests.http` in VS Code with the REST Client extension and click "Send Request" above each request.

### Option 2: Using cURL

```bash
# List all documents (as Admin)
curl -X GET "https://localhost:7000/api/documents?page=1&pageSize=20" \
  -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "X-User-Role: Admin" \
  -H "X-Department-Id: 11111111-1111-1111-1111-111111111111"
  
# Create a document
curl -X POST "https://localhost:7000/api/documents" \
  -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "X-User-Role: Admin" \
  -H "X-Department-Id: 11111111-1111-1111-1111-111111111111" \
  -H "Content-Type: application/json" \
  -d '{"title":"Test Doc","category":"Finance","tags":["test"],"departmentId":"22222222-2222-2222-2222-222222222222"}'
```

### Option 3: Using Postman

Import the API by creating requests manually or use the examples from `DocumentTests.http`.

## 🔐 Authentication

This API uses **mock header-based authentication** for development and testing purposes. Authentication is performed by reading user information from HTTP headers instead of JWT tokens.

### Required Headers

Every authenticated request must include these three headers:

| Header | Description | Example |
|--------|-------------|---------|
| `X-User-Id` | User GUID | `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` |
| `X-User-Role` | Role (Admin, Editor, or Viewer) | `Admin` |
| `X-Department-Id` | Department GUID | `11111111-1111-1111-1111-111111111111` |

### Example Request

```bash
curl -X GET "https://localhost:7000/api/documents" \
  -H "X-User-Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "X-User-Role: Admin" \
  -H "X-Department-Id: 11111111-1111-1111-1111-111111111111"
```

### Pre-configured Test Users

**Admin (IT Department):**
- X-User-Id: `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`
- X-User-Role: `Admin`
- X-Department-Id: `11111111-1111-1111-1111-111111111111`

**Editor (Finance Department):**
- X-User-Id: `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb`
- X-User-Role: `Editor`
- X-Department-Id: `22222222-2222-2222-2222-222222222222`

**Viewer (HR Department):**
- X-User-Id: `cccccccc-cccc-cccc-cccc-cccccccccccc`
- X-User-Role: `Viewer`
- X-Department-Id: `33333333-3333-3333-3333-333333333333`

### Validation Rules

- ✅ All three headers are **required**
- ✅ `X-User-Id` must be a valid GUID
- ✅ `X-User-Role` must be one of: `Admin`, `Editor`, or `Viewer`
- ✅ `X-Department-Id` must be a valid GUID
- ❌ Missing, invalid, or malformed headers will result in **401 Unauthorized**

> ⚠️ **Note**: This authentication mechanism is for development/testing only and should NOT be used in production.

## 📡 API Endpoints

### Documents

| Method | Endpoint | Description | Required Role |
|--------|----------|-------------|---------------|
| `GET` | `/api/documents` | List documents (paginated) | Any (filtered by role) |
| `GET` | `/api/documents/{id}` | Get single document | Any (access controlled) |
| `POST` | `/api/documents` | Create document metadata | Any |
| `PUT` | `/api/documents/{id}` | Update document | Editor or Admin |
| `DELETE` | `/api/documents/{id}` | Soft delete document | Editor or Admin |
| `GET` | `/api/documents/{id}/download` | Download file | Any (access controlled) |

### Query Parameters for GET /api/documents

- `page` (default: 1): Page number
- `pageSize` (default: 20, max: 100): Items per page
- `category` (optional): Filter by category

### Response Headers

- `X-Total-Count`: Total number of documents (for pagination)

## 👥 Role-Based Access Control

| Role | Permissions |
|------|-------------|
| **Admin** | Full CRUD on ALL documents + download all |
| **Editor** | Read and download ALL documents, UPDATE and DELETE |
| **Viewer** | Read and download ONLY documents from their department |

### Access Rules

- Viewers can only see documents where `DepartmentId` matches their token's `departmentId` claim
- Soft-deleted documents are hidden from all listings
- Attempting to access unauthorized documents returns `404 Not Found` (not `403`) to prevent information disclosure

## 📦 Project Structure

```
DocumentManagement/
├── DocumentManagement.Domain/
│   ├── Entities/
│   │   ├── BaseEntity.cs
│   │   ├── Department.cs
│   │   └── Document.cs
│   └── Interfaces/
│       ├── IDocumentRepository.cs
│       └── IFileStorageService.cs
├── DocumentManagement.Application/
│   ├── DTOs/
│   │   ├── DocumentDto.cs
│   │   ├── CreateDocumentDto.cs
│   │   ├── UpdateDocumentDto.cs
│   │   └── PagedResultDto.cs
│   ├── Validators/
│   │   ├── CreateDocumentValidator.cs
│   │   └── UpdateDocumentValidator.cs
│   ├── Services/
│   │   └── DocumentService.cs
│   └── Interfaces/
│       └── IDocumentService.cs
├── DocumentManagement.Infrastructure/
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   ├── Repositories/
│   │   └── DocumentRepository.cs
│   └── Services/
│       └── FileStorageService.cs
└── DocumentManagement.Api/
    ├── Controllers/
    │   └── DocumentsController.cs
    ├── Extensions/
    │   └── ClaimsPrincipalExtensions.cs
    ├── Middleware/
    │   ├── ExceptionHandlingMiddleware.cs
    │   └── MockAuthenticationHandler.cs
    ├── Helpers/
    │   ├── ApplicationBuilderExtensions.cs
    │   ├── JwtTokenGenerator.cs (legacy, not used)
    │   └── ServiceCollectionExtensions.cs
    ├── AppData/Documents/
    ├── appsettings.json
    └── Program.cs
```

## 🗄️ Database Schema

### Departments Table
- Id (GUID, PK)
- Name (string, 100)
- CreatedAt (DateTime)
- UpdatedAt (DateTime, nullable)
- IsDeleted (bool)

### Documents Table
- Id (GUID, PK)
- Title (string, 200, required)
- Description (string, 2000, nullable)
- Category (string, 100, required)
- Tags (JSON array)
- FilePath (string, 500, required)
- FileSize (bigint)
- MimeType (string, 100)
- DepartmentId (GUID, FK)
- CreatedBy (GUID)
- CreatedAt (DateTime)
- UpdatedAt (DateTime, nullable)
- IsDeleted (bool)

## 🎯 Key Features Implemented

✅ Clean Architecture with 4 projects  
✅ Entity Framework Core with SQL Server  
✅ Global query filters for soft delete  
✅ Mock header-based authentication (custom AuthenticationHandler)  
✅ Policy-based authorization  
✅ Role-based access control (Admin, Editor, Viewer)  
✅ Department-scoped data filtering for Viewers  
✅ FluentValidation for DTO validation  
✅ Efficient file streaming (PhysicalFileResult)  
✅ Pagination with total count  
✅ Proper HTTP status codes  
✅ Exception handling middleware  
✅ Database seeding with sample data  

## 🔧 Technical Stack

- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: SQL Server (LocalDB)
- **Authentication**: Custom header-based authentication (MockAuthenticationHandler)
- **Validation**: FluentValidation 11.3
- **Documentation**: OpenAPI/Swagger (commented out due to package conflict)

## 📝 Assumptions Made

1. **File Upload Not Implemented**: The assignment sample requests only show metadata creation, not file upload. Documents reference pre-seeded files in `AppData/Documents`.  

2. **Mock Header-Based Authentication**: Uses custom authentication handler that reads user information from HTTP headers (`X-User-Id`, `X-User-Role`, `X-Department-Id`). This is for development/testing only.

3. **LocalDB for Development**: Uses SQL Server LocalDB for simplicity. Can be changed to full SQL Server or Azure SQL.

4. **Soft Delete Only**: Documents are never physically deleted from the database, only marked as `IsDeleted = true`.

5. **Static File Storage**: Files stored locally in `AppData/Documents`. For production, use Azure Blob Storage or AWS S3.

6. **Swagger Temporarily Disabled**: Due to package version conflicts between Microsoft.OpenApi.Models namespaces. Can be resolved by downgrading to .NET 7 Swashbuckle packages or waiting for compatibility updates.

## 🐛 Known Issues

- **Swagger UI**: Commented out due to Microsoft.OpenApi.Models namespace compatibility issue with .NET 8. Use `DocumentTests.http` file instead.
- **EF Core Warning**: "Tags property has value converter but no value comparer" - benign warning, doesn't affect functionality.

## 🚀 Future Enhancements

- [ ] Add actual file upload endpoint with multipart/form-data
- [ ] Implement production-ready authentication (OAuth2, Azure AD, etc.)
- [ ] Add comprehensive unit tests
- [ ] Add integration tests
- [ ] Implement caching (Redis)
- [ ] Add audit logging
- [ ] Implement file versioning
- [ ] Add fulltext search
- [ ] Docker containerization
- [ ] CI/CD pipeline

## 👨‍💻 Development

### Running Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName --project DocumentManagement.Infrastructure --startup-project DocumentManagement.Api

# Apply migrations
dotnet ef database update --project DocumentManagement.Api

# Remove last migration
dotnet ef migrations remove --project DocumentManagement.Infrastructure --startup-project DocumentManagement.Api
```

### Resetting the Database

```bash
dotnet ef database drop --project DocumentManagement.Api --force
dotnet ef database update --project DocumentManagement.Api
```

## 📧 Contact

For questions about this implementation, please contact your recruiter.

---

**Built with ❤️ for Knowledge Management Platform**
