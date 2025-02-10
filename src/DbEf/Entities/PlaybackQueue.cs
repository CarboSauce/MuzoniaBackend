using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public enum RepeatMode
{
    None,
    One,
    All,
}

public class PlaybackQueue : Entity
{
    public required EntityId OwnerId { get; set; }
    public AppUser Owner { get; set; } = null!;
    public required bool IsPublic { get; set; }
    public required bool IsModifiable { get; set; }
    public required bool IsRepeat { get; set; }
    public required bool IsPlaying { get; set; }
    public required bool IsRandom { get; set; }
    public required int CurrentIndex { get; set; }
    public required int TrackCount { get; set; }
    public required int Timestamp { get; set; }
    public virtual ICollection<QueueEntry> Entries { get; } = null!;
    public ICollection<QueueUser> QueueUsers { get; } = null!;
}

public class UserQueueConfig : IEntityTypeConfiguration<PlaybackQueue>
{
    public void Configure(EntityTypeBuilder<PlaybackQueue> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasMany(e => e.Entries).WithOne(e => e.Queue);
        builder.HasOne(e => e.Owner).WithOne(e => e.Queue);
        builder.HasIndex(e => e.OwnerId).IsUnique();
    }
}
