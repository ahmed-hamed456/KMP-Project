-- Seed Data for SearchService Database
-- 25 documents across 5 departments

USE SearchServiceDb;
GO

-- Clear any existing test data
DELETE FROM SearchableDocuments;
GO

-- Finance Department Documents (5 documents)
INSERT INTO SearchableDocuments (Id, Title, Description, Category, Tags, DepartmentId, DepartmentName, IsDeleted, LastSyncedAt, CreatedAt, UpdatedAt)
VALUES
('11111111-1111-1111-1111-000000000001', 'Q4 2025 Financial Report', 'Comprehensive financial analysis for the fourth quarter including revenue, expenses, and profit margins', 'Finance', 'quarterly,financial,report,revenue,Q4', 'D1111111-1111-1111-1111-111111111111', 'Finance Department', 0, GETUTCDATE(), DATEADD(day, -90, GETUTCDATE()), DATEADD(day, -85, GETUTCDATE())),
('11111111-1111-1111-1111-000000000002', 'Annual Budget Planning 2026', 'Strategic budget allocation and financial planning for fiscal year 2026', 'Finance', 'budget,planning,annual,strategy,2026', 'D1111111-1111-1111-1111-111111111111', 'Finance Department', 0, GETUTCDATE(), DATEADD(day, -75, GETUTCDATE()), DATEADD(day, -70, GETUTCDATE())),
('11111111-1111-1111-1111-000000000003', 'Tax Compliance Guidelines', 'Updated tax compliance procedures and regulatory requirements for 2026', 'Finance', 'tax,compliance,regulations,guidelines', 'D1111111-1111-1111-1111-111111111111', 'Finance Department', 0, GETUTCDATE(), DATEADD(day, -60, GETUTCDATE()), DATEADD(day, -55, GETUTCDATE())),
('11111111-1111-1111-1111-000000000004', 'Invoice Processing Workflow', 'Automated invoice processing system documentation and best practices', 'Finance', 'invoice,automation,workflow,payment', 'D1111111-1111-1111-1111-111111111111', 'Finance Department', 0, GETUTCDATE(), DATEADD(day, -45, GETUTCDATE()), DATEADD(day, -40, GETUTCDATE())),
('11111111-1111-1111-1111-000000000005', 'Expense Reimbursement Policy', 'Updated employee expense reimbursement policy and submission procedures', 'Finance', 'expense,reimbursement,policy,employee', 'D1111111-1111-1111-1111-111111111111', 'Finance Department', 0, GETUTCDATE(), DATEADD(day, -30, GETUTCDATE()), DATEADD(day, -25, GETUTCDATE()));
GO

-- HR Department Documents (5 documents)
INSERT INTO SearchableDocuments (Id, Title, Description, Category, Tags, DepartmentId, DepartmentName, IsDeleted, LastSyncedAt, CreatedAt, UpdatedAt)
VALUES
('22222222-2222-2222-2222-000000000001', 'Employee Onboarding Guide', 'Comprehensive guide for new employee onboarding process and orientation', 'HR', 'onboarding,orientation,employee,new-hire', 'D2222222-2222-2222-2222-222222222222', 'HR Department', 0, GETUTCDATE(), DATEADD(day, -80, GETUTCDATE()), DATEADD(day, -75, GETUTCDATE())),
('22222222-2222-2222-2222-000000000002', 'Performance Review Framework 2026', 'Annual performance evaluation criteria and review process guidelines', 'HR', 'performance,review,evaluation,annual', 'D2222222-2222-2222-2222-222222222222', 'HR Department', 0, GETUTCDATE(), DATEADD(day, -65, GETUTCDATE()), DATEADD(day, -60, GETUTCDATE())),
('22222222-2222-2222-2222-000000000003', 'Remote Work Policy', 'Work from home guidelines, equipment requirements, and productivity standards', 'HR', 'remote,work-from-home,policy,flexibility', 'D2222222-2222-2222-2222-222222222222', 'HR Department', 0, GETUTCDATE(), DATEADD(day, -50, GETUTCDATE()), DATEADD(day, -45, GETUTCDATE())),
('22222222-2222-2222-2222-000000000004', 'Benefits Enrollment Guide', 'Healthcare, retirement, and insurance benefits enrollment instructions', 'HR', 'benefits,healthcare,insurance,enrollment', 'D2222222-2222-2222-2222-222222222222', 'HR Department', 0, GETUTCDATE(), DATEADD(day, -35, GETUTCDATE()), DATEADD(day, -30, GETUTCDATE())),
('22222222-2222-2222-2222-000000000005', 'Diversity and Inclusion Initiative', 'DEI program overview, goals, and implementation strategy', 'HR', 'diversity,inclusion,DEI,culture', 'D2222222-2222-2222-2222-222222222222', 'HR Department', 0, GETUTCDATE(), DATEADD(day, -20, GETUTCDATE()), DATEADD(day, -15, GETUTCDATE()));
GO

-- Engineering Department Documents (5 documents)
INSERT INTO SearchableDocuments (Id, Title, Description, Category, Tags, DepartmentId, DepartmentName, IsDeleted, LastSyncedAt, CreatedAt, UpdatedAt)
VALUES
('33333333-3333-3333-3333-000000000001', 'API Development Standards', 'RESTful API design guidelines, versioning, and documentation requirements', 'Engineering', 'API,development,standards,REST,guidelines', 'D3333333-3333-3333-3333-333333333333', 'Engineering Department', 0, GETUTCDATE(), DATEADD(day, -85, GETUTCDATE()), DATEADD(day, -80, GETUTCDATE())),
('33333333-3333-3333-3333-000000000002', 'Code Review Best Practices', 'Peer code review process, checklist, and quality standards', 'Engineering', 'code-review,quality,best-practices,peer-review', 'D3333333-3333-3333-3333-333333333333', 'Engineering Department', 0, GETUTCDATE(), DATEADD(day, -70, GETUTCDATE()), DATEADD(day, -65, GETUTCDATE())),
('33333333-3333-3333-3333-000000000003', 'Docker Containerization Guide', 'Container deployment strategies and Kubernetes orchestration', 'Engineering', 'docker,containers,kubernetes,deployment', 'D3333333-3333-3333-3333-333333333333', 'Engineering Department', 0, GETUTCDATE(), DATEADD(day, -55, GETUTCDATE()), DATEADD(day, -50, GETUTCDATE())),
('33333333-3333-3333-3333-000000000004', 'CI/CD Pipeline Documentation', 'Automated build, test, and deployment pipeline configuration', 'Engineering', 'CI/CD,automation,pipeline,DevOps', 'D3333333-3333-3333-3333-333333333333', 'Engineering Department', 0, GETUTCDATE(), DATEADD(day, -40, GETUTCDATE()), DATEADD(day, -35, GETUTCDATE())),
('33333333-3333-3333-3333-000000000005', 'Database Migration Strategy', 'Zero-downtime database schema migration procedures', 'Engineering', 'database,migration,schema,zero-downtime', 'D3333333-3333-3333-3333-333333333333', 'Engineering Department', 0, GETUTCDATE(), DATEADD(day, -25, GETUTCDATE()), DATEADD(day, -20, GETUTCDATE()));
GO

-- Marketing Department Documents (5 documents)
INSERT INTO SearchableDocuments (Id, Title, Description, Category, Tags, DepartmentId, DepartmentName, IsDeleted, LastSyncedAt, CreatedAt, UpdatedAt)
VALUES
('44444444-4444-4444-4444-000000000001', 'Q1 2026 Marketing Campaign', 'Social media and digital marketing strategy for first quarter', 'Marketing', 'campaign,social-media,digital,strategy,Q1', 'D4444444-4444-4444-4444-444444444444', 'Marketing Department', 0, GETUTCDATE(), DATEADD(day, -75, GETUTCDATE()), DATEADD(day, -70, GETUTCDATE())),
('44444444-4444-4444-4444-000000000002', 'Brand Guidelines 2026', 'Official brand identity, logo usage, and style guide', 'Marketing', 'brand,identity,logo,style-guide', 'D4444444-4444-4444-4444-444444444444', 'Marketing Department', 0, GETUTCDATE(), DATEADD(day, -60, GETUTCDATE()), DATEADD(day, -55, GETUTCDATE())),
('44444444-4444-4444-4444-000000000003', 'Content Marketing Strategy', 'Blog posts, whitepapers, and thought leadership content plan', 'Marketing', 'content,blog,whitepaper,thought-leadership', 'D4444444-4444-4444-4444-444444444444', 'Marketing Department', 0, GETUTCDATE(), DATEADD(day, -45, GETUTCDATE()), DATEADD(day, -40, GETUTCDATE())),
('44444444-4444-4444-4444-000000000004', 'Customer Analytics Report', 'Customer behavior analysis and market segmentation insights', 'Marketing', 'analytics,customer,segmentation,insights', 'D4444444-4444-4444-4444-444444444444', 'Marketing Department', 0, GETUTCDATE(), DATEADD(day, -30, GETUTCDATE()), DATEADD(day, -25, GETUTCDATE())),
('44444444-4444-4444-4444-000000000005', 'Email Marketing Automation', 'Automated email campaign setup and subscriber engagement tactics', 'Marketing', 'email,automation,campaign,engagement', 'D4444444-4444-4444-4444-444444444444', 'Marketing Department', 0, GETUTCDATE(), DATEADD(day, -15, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()));
GO

-- Legal Department Documents (5 documents)
INSERT INTO SearchableDocuments (Id, Title, Description, Category, Tags, DepartmentId, DepartmentName, IsDeleted, LastSyncedAt, CreatedAt, UpdatedAt)
VALUES
('55555555-5555-5555-5555-000000000001', 'Data Privacy Compliance GDPR', 'GDPR compliance requirements and data protection procedures', 'Legal', 'GDPR,privacy,compliance,data-protection', 'D5555555-5555-5555-5555-555555555555', 'Legal Department', 0, GETUTCDATE(), DATEADD(day, -80, GETUTCDATE()), DATEADD(day, -75, GETUTCDATE())),
('55555555-5555-5555-5555-000000000002', 'Standard Contract Templates', 'Pre-approved contract templates for vendor and client agreements', 'Legal', 'contract,template,vendor,agreement', 'D5555555-5555-5555-5555-555555555555', 'Legal Department', 0, GETUTCDATE(), DATEADD(day, -65, GETUTCDATE()), DATEADD(day, -60, GETUTCDATE())),
('55555555-5555-5555-5555-000000000003', 'Intellectual Property Guidelines', 'Patent, trademark, and copyright protection procedures', 'Legal', 'IP,patent,trademark,copyright', 'D5555555-5555-5555-5555-555555555555', 'Legal Department', 0, GETUTCDATE(), DATEADD(day, -50, GETUTCDATE()), DATEADD(day, -45, GETUTCDATE())),
('55555555-5555-5555-5555-000000000004', 'Regulatory Compliance Checklist', 'Industry regulations and compliance audit requirements', 'Legal', 'compliance,regulations,audit,checklist', 'D5555555-5555-5555-5555-555555555555', 'Legal Department', 0, GETUTCDATE(), DATEADD(day, -35, GETUTCDATE()), DATEADD(day, -30, GETUTCDATE())),
('55555555-5555-5555-5555-000000000005', 'Non-Disclosure Agreement NDA', 'Confidentiality agreement template and signing procedures', 'Legal', 'NDA,confidentiality,agreement,disclosure', 'D5555555-5555-5555-5555-555555555555', 'Legal Department', 0, GETUTCDATE(), DATEADD(day, -20, GETUTCDATE()), DATEADD(day, -15, GETUTCDATE()));
GO

-- Verify the seed data
SELECT 
    Category, 
    COUNT(*) as DocumentCount
FROM SearchableDocuments
WHERE IsDeleted = 0
GROUP BY Category
ORDER BY Category;
GO

SELECT COUNT(*) as TotalDocuments FROM SearchableDocuments WHERE IsDeleted = 0;
GO
