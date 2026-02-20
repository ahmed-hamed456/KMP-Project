using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DocumentManagement.Domain.Entities;

namespace DocumentManagement.Infrastructure.Data;

public class DataSeeder
{
    private readonly ILogger _logger;
    
    // Predefined IDs for consistency
    private readonly Guid _itDeptId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _financeDeptId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly Guid _hrDeptId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly Guid _adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    
    public DataSeeder(ILogger logger)
    {
        _logger = logger;
    }
    
    public async Task SeedAsync(AppDbContext context)
    {
        await SeedDepartmentsAsync(context);
        await SeedDocumentsAsync(context);
        await CreateSampleFilesAsync();
        
        await context.SaveChangesAsync();
    }
    
    private async Task SeedDepartmentsAsync(AppDbContext context)
    {
        if (await context.Departments.AnyAsync())
        {
            _logger.LogInformation("Departments already exist. Skipping department seeding.");
            return;
        }
        
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var departments = new List<Department>
        {
            new Department
            {
                Id = _itDeptId,
                Name = "IT",
                CreatedAt = seedDate,
                IsDeleted = false
            },
            new Department
            {
                Id = _financeDeptId,
                Name = "Finance",
                CreatedAt = seedDate,
                IsDeleted = false
            },
            new Department
            {
                Id = _hrDeptId,
                Name = "HR",
                CreatedAt = seedDate,
                IsDeleted = false
            }
        };
        
        await context.Departments.AddRangeAsync(departments);
        _logger.LogInformation("Seeded {Count} departments.", departments.Count);
    }
    
    private async Task SeedDocumentsAsync(AppDbContext context)
    {
        if (await context.Documents.AnyAsync())
        {
            _logger.LogInformation("Documents already exist. Skipping document seeding.");
            return;
        }
        
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var documents = new List<Document>
        {
            new Document
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                Title = "Q4 Financial Report",
                Description = "Quarterly financial summary for Q4 2024",
                Category = "Finance",
                Tags = new List<string> { "quarterly", "financial", "2024" },
                FilePath = "q4-financial-report.txt",
                FileSize = 350,
                MimeType = "text/plain",
                DepartmentId = _financeDeptId,
                CreatedBy = _adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false
            },
            new Document
            {
                Id = Guid.Parse("d2222222-2222-2222-2222-222222222222"),
                Title = "IT Infrastructure Plan 2025",
                Description = "Strategic plan for IT infrastructure improvements",
                Category = "Technical",
                Tags = new List<string> { "strategy", "infrastructure", "2025" },
                FilePath = "it-infrastructure-plan.pdf",
                FileSize = 950,
                MimeType = "application/pdf",
                DepartmentId = _itDeptId,
                CreatedBy = _adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false
            },
            new Document
            {
                Id = Guid.Parse("d3333333-3333-3333-3333-333333333333"),
                Title = "Employee Handbook 2025",
                Description = "Updated employee policies and procedures",
                Category = "HR",
                Tags = new List<string> { "policies", "handbook", "2025" },
                FilePath = "employee-handbook-2025.json",
                FileSize = 720,
                MimeType = "application/json",
                DepartmentId = _hrDeptId,
                CreatedBy = _adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false
            },
            new Document
            {
                Id = Guid.Parse("d4444444-4444-4444-4444-444444444444"),
                Title = "Budget Analysis Q1",
                Description = "First quarter budget analysis and projections",
                Category = "Finance",
                Tags = new List<string> { "budget", "analysis", "Q1" },
                FilePath = "q1-budget-analysis.xml",
                FileSize = 540,
                MimeType = "application/xml",
                DepartmentId = _financeDeptId,
                CreatedBy = _adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false
            },
            new Document
            {
                Id = Guid.Parse("d5555555-5555-5555-5555-555555555555"),
                Title = "Network Security Guidelines",
                Description = "Security best practices for network administration",
                Category = "Technical",
                Tags = new List<string> { "security", "network", "guidelines" },
                FilePath = "network-security-guidelines.csv",
                FileSize = 280,
                MimeType = "text/csv",
                DepartmentId = _itDeptId,
                CreatedBy = _adminUserId,
                CreatedAt = seedDate,
                IsDeleted = false
            }
        };
        
        await context.Documents.AddRangeAsync(documents);
        _logger.LogInformation("Seeded {Count} documents.", documents.Count);
    }
    
    private async Task CreateSampleFilesAsync()
    {
        var documentsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "Documents");
        Directory.CreateDirectory(documentsPath);

        // TXT: Q4 Financial Report
        var txtPath = Path.Combine(documentsPath, "q4-financial-report.txt");
        if (!File.Exists(txtPath))
        {
            var content = @"Q4 Financial Report

Quarterly Financial Summary for Q4 2024

Revenue: $5,234,567
Expenses: $3,456,789
Net Income: $1,777,778

Document Management System - Finance Department";
            await File.WriteAllTextAsync(txtPath, content);
            _logger.LogInformation("Created sample TXT file: {FilePath}", txtPath);
        }

        // PDF: IT Infrastructure Plan 2025
        var pdfPath = Path.Combine(documentsPath, "it-infrastructure-plan.pdf");
        if (!File.Exists(pdfPath))
        {
            var pdfBase64 = @"JVBERi0xLjQKMSAwIG9iago8PAovVHlwZSAvQ2F0YWxvZwovUGFnZXMgMiAwIFIKPj4KZW5kb2JqCjIgMCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9LaWRzIFszIDAgUl0KL0NvdW50IDEKL01lZGlhQm94IFswIDAgNjEyIDc5Ml0KPj4KZW5kb2JqCjMgMCBvYmoKPDwKL1R5cGUgL1BhZ2UKL1BhcmVudCAyIDAgUgovUmVzb3VyY2VzIDw8Ci9Gb250IDw8Ci9GMSA8PAovVHlwZSAvRm9udAovU3VidHlwZSAvVHlwZTEKL0Jhc2VGb250IC9IZWx2ZXRpY2EtQm9sZAo+PgovRjIgPDwKL1R5cGUgL0ZvbnQKL1N1YnR5cGUgL1R5cGUxCi9CYXNlRm9udCAvSGVsdmV0aWNhCj4+Cj4+Cj4+Ci9Db250ZW50cyA0IDAgUgo+PgplbmRvYmoKNCAwIG9iago8PAovTGVuZ3RoIDM5MAo+PgpzdHJlYW0KQlQKL0YxIDI0IFRmCjUwIDc1MCBUZAooSVQgSW5mcmFzdHJ1Y3R1cmUgUGxhbiAyMDI1KSBUagowIC00MCBUZAovRjIgMTIgVGYKKFN0cmF0ZWdpYyBwbGFuIGZvciBJVCBpbmZyYXN0cnVjdHVyZSBpbXByb3ZlbWVudHMpIFRqCjAgLTMwIFRkCi9GMSA0IFRmCihLZXkgSW5pdGlhdGl2ZXM6KSBUagowIC0yMCBUZAovRjIgMTIgVGYKKC0gQ2xvdWQgTWlncmF0aW9uKSBUagowIC0xNSBUZAooLSBDeWJlcnNlY3VyaXR5IEVuaGFuY2VtZW50KSBUagowIC0xNSBUZAooLSBOZXR3b3JrIE1vZGVybml6YXRpb24pIFRqCjAgLTMwIFRkCihCdWRnZXQ6ICQyLDEwMCwwMDApIFRqCjAgLTMwIFRkCi9GMiAxMCBUZgooRG9jdW1lbnQgTWFuYWdlbWVudCBTeXN0ZW0gLSBJVCkgVGoKRVQKZW5kc3RyZWFtCmVuZG9iagp4cmVmCjAgNQowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwMDkgMDAwMDAgbiAKMDAwMDAwMDA1OCAwMDAwMCBuIAowMDAwMDAwMTQ3IDAwMDAwIG4gCjAwMDAwMDAzNTcgMDAwMDAgbiAKdHJhaWxlcgo8PAovU2l6ZSA1Ci9Sb290IDEgMCBSCj4+CnN0YXJ0eHJlZgo3OTgKJSVFT0YK";
            var pdfBytes = Convert.FromBase64String(pdfBase64);
            await File.WriteAllBytesAsync(pdfPath, pdfBytes);
            _logger.LogInformation("Created sample PDF file: {FilePath}", pdfPath);
        }

        // JSON: Employee Handbook 2025
        var jsonPath = Path.Combine(documentsPath, "employee-handbook-2025.json");
        if (!File.Exists(jsonPath))
        {
            var jsonContent = @"{
  ""title"": ""Employee Handbook 2025"",
  ""version"": ""1.0"",
  ""effectiveDate"": ""2025-01-01"",
  ""sections"": [
    {
      ""id"": 1,
      ""name"": ""Code of Conduct"",
      ""description"": ""Expected behavior and ethical standards""
    },
    {
      ""id"": 2,
      ""name"": ""Leave Policy"",
      ""description"": ""Vacation, sick leave, and time off procedures""
    },
    {
      ""id"": 3,
      ""name"": ""Benefits"",
      ""description"": ""Health insurance, retirement plans, and perks""
    },
    {
      ""id"": 4,
      ""name"": ""Workplace Safety"",
      ""description"": ""Safety protocols and emergency procedures""
    }
  ],
  ""department"": ""HR"",
  ""approved"": true
}";
            await File.WriteAllTextAsync(jsonPath, jsonContent);
            _logger.LogInformation("Created sample JSON file: {FilePath}", jsonPath);
        }

        // XML: Budget Analysis Q1
        var xmlPath = Path.Combine(documentsPath, "q1-budget-analysis.xml");
        if (!File.Exists(xmlPath))
        {
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<BudgetAnalysis>
  <Quarter>Q1</Quarter>
  <Year>2025</Year>
  <Department>Finance</Department>
  <Summary>
    <AllocatedBudget>2500000</AllocatedBudget>
    <ActualSpending>2350000</ActualSpending>
    <Variance>150000</Variance>
    <VariancePercentage>6.0</VariancePercentage>
  </Summary>
  <Categories>
    <Category name=""Operations"" allocated=""1500000"" spent=""1420000"" />
    <Category name=""Marketing"" allocated=""500000"" spent=""480000"" />
    <Category name=""Technology"" allocated=""500000"" spent=""450000"" />
  </Categories>
</BudgetAnalysis>";
            await File.WriteAllTextAsync(xmlPath, xmlContent);
            _logger.LogInformation("Created sample XML file: {FilePath}", xmlPath);
        }

        // CSV: Network Security Guidelines
        var csvPath = Path.Combine(documentsPath, "network-security-guidelines.csv");
        if (!File.Exists(csvPath))
        {
            var csvContent = @"Guideline ID,Category,Severity,Description,Compliance Required
SEC-001,Access Control,High,Implement multi-factor authentication,Yes
SEC-002,Network,High,Enable firewall on all endpoints,Yes
SEC-003,Data,Critical,Encrypt sensitive data at rest,Yes
SEC-004,Patching,Medium,Monthly security patch updates,Yes
SEC-005,Monitoring,Medium,Real-time intrusion detection,No";
            await File.WriteAllTextAsync(csvPath, csvContent);
            _logger.LogInformation("Created sample CSV file: {FilePath}", csvPath);
        }
    }
}
