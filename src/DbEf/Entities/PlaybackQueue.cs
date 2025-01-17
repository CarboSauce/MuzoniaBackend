using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public enum RepeatMode
{
    None,
    RepeatOne,
    RepeatAll
}

public class PlaybackQueue : Entity
{
    public required EntityId UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required RepeatMode RepeatMode { get; set; }
    public required bool IsRandom { get; set; }
    public required long CurrentIndex { get; set; }
    public required long Timestamp { get; set; }
    public virtual ICollection<QueueEntry> Entries { get; } = null!;
}

public class UserQueueConfig : IEntityTypeConfiguration<PlaybackQueue>
{
    public void Configure(EntityTypeBuilder<PlaybackQueue> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasMany(e => e.Entries).WithOne(e => e.Queue);
    }
}
