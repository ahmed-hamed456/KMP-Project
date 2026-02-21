using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DocumentManagement.Domain.Entities;

namespace DocumentManagement.Infrastructure.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);
        
        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(d => d.Description)
            .HasMaxLength(2000);
        
        builder.Property(d => d.Category)
            .IsRequired()
            .HasMaxLength(100);
        
        // Store Tags as JSON
        builder.Property(d => d.Tags)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
            .HasColumnType("nvarchar(max)");
        
        builder.Property(d => d.FilePath)
            .IsRequired()
            .HasMaxLength(500);
        
        builder.Property(d => d.MimeType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(d => d.FileSize)
            .IsRequired();
        
        builder.Property(d => d.DepartmentId)
            .IsRequired();
        
        builder.Property(d => d.CreatedBy)
            .IsRequired();
        
        builder.Property(d => d.CreatedAt)
            .IsRequired();
        
        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        // Indexes
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.DepartmentId);
        builder.HasIndex(d => d.IsDeleted);
    }
}
