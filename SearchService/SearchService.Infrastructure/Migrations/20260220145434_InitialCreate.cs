using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchableDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DepartmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartmentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchableDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchableDocuments_Category",
                table: "SearchableDocuments",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_SearchableDocuments_CreatedAt",
                table: "SearchableDocuments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SearchableDocuments_DepartmentId",
                table: "SearchableDocuments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchableDocuments_IsDeleted",
                table: "SearchableDocuments",
                column: "IsDeleted");

            // Create Full-Text Catalog (must run outside transaction)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'SearchCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG SearchCatalog AS DEFAULT;
                END
            ", suppressTransaction: true);

            // Create Full-Text Index on Title, Description, Tags (must run outside transaction)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('SearchableDocuments'))
                BEGIN
                    CREATE FULLTEXT INDEX ON SearchableDocuments(Title, Description, Tags)
                    KEY INDEX PK_SearchableDocuments
                    ON SearchCatalog
                    WITH CHANGE_TRACKING AUTO;
                END
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Full-Text Index (must run outside transaction)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('SearchableDocuments'))
                BEGIN
                    DROP FULLTEXT INDEX ON SearchableDocuments;
                END
            ", suppressTransaction: true);

            // Drop Full-Text Catalog (must run outside transaction)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'SearchCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG SearchCatalog;
                END
            ", suppressTransaction: true);

            migrationBuilder.DropTable(
                name: "SearchableDocuments");
        }
    }
}
