using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class FileModelMetadata : Entity
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public required string ContentType { get; set; }
    public required int Length { get; set; }
}

public sealed class FileModel : FileModelMetadata
{
    public required byte[] Data { get; set; }
}

public sealed class FileConfig : IEntityTypeConfiguration<FileModel>
{
    public void Configure(EntityTypeBuilder<FileModel> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Path).HasMaxLength(255).IsRequired();
        builder.Property(e => e.ContentType).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CreationDate).IsRequired();
        // MaxLength is 30MiB
        // Postgres doesn't seem to respect this anyway
        // builder.Property(e => e.Data).IsRequired().HasMaxLength(31_457_280);
        // Index
        builder.HasIndex(e => e.Path).IsUnique();
    }
}
