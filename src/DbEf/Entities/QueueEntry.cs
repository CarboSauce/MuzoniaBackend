using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class QueueEntry
{
    public EntityId Id { get; set; } = NewId.Create();
    public required EntityId TrackId { get; set; }
    public Track Track { get; set; } = null!;
    public required int Index { get; set; }
    public EntityId QueueId { get; set; }
    public virtual PlaybackQueue Queue { get; set; } = null!;
}

public class QueueConfig : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Queue).WithMany(e => e.Entries);
        builder.HasOne(e => e.Track).WithMany();
    }
}
