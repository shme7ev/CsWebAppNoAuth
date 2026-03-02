using Microsoft.EntityFrameworkCore;
using FileStorageService.Models;

namespace FileStorageService.Data;

public class FileStorageDbContext : DbContext
{
    public FileStorageDbContext(DbContextOptions<FileStorageDbContext> options) : base(options)
    {
    }

    public DbSet<FileMetadata> Files { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(entity =>
        {
            entity.ToTable("files");
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("file_name");

            entity.Property(e => e.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("original_file_name");

            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("content_type");

            entity.Property(e => e.Size)
                .IsRequired()
                .HasColumnName("size");

            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("file_path");

            entity.Property(e => e.UploadDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("upload_date");

            entity.Property(e => e.UploadedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("uploaded_by");

            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .HasColumnName("description");

            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");

            entity.Property(e => e.DeletedAt)
                .HasColumnName("deleted_at");

            entity.Property(e => e.DeletedBy)
                .HasMaxLength(100)
                .HasColumnName("deleted_by");

            // Indexes
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.UploadDate);
            entity.HasIndex(e => e.UploadedBy);
            entity.HasIndex(e => e.IsDeleted);
        });

        // Seed some initial data if needed
        // SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder) { }
}
