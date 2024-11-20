using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class QueueEntry
{
    public EntityId Id { get; set; } = NewId.Create();
    public required EntityId UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required EntityId SongId { get; set; }
    public Track Song { get; set; } = null!;
    public required long Index { get; set; }
}

public class QueueConfig : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User).WithMany();
        builder.HasOne(e => e.Song).WithMany();
    }
}
