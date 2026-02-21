using Microsoft.EntityFrameworkCore;
using SearchService.Domain.Entities;

namespace SearchService.Infrastructure.Data;

public class SearchDbContext : DbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
    {
    }

    public DbSet<SearchableDocument> SearchableDocuments => Set<SearchableDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SearchableDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Tags)
                .HasMaxLength(1000);

            entity.Property(e => e.DepartmentName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.LastSyncedAt)
                .IsRequired();

            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.DepartmentId);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.CreatedAt);
        });
    }
}
