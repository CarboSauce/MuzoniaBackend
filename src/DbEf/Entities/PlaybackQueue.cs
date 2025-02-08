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
    public required EntityId UserId { get; set; }
    public required EntityId? DeviceId { get; set; }
    public Device? Device { get; } = null!;
    public AppUser User { get; set; } = null!;
    public required bool IsRepeat { get; set; }
    public required int Volume { get; set; }
    public required bool IsPlaying { get; set; }
    public required bool IsRandom { get; set; }
    public required int CurrentIndex { get; set; }
    public required int TrackCount { get; set; }
    public required int Timestamp { get; set; }
    public virtual ICollection<QueueEntry> Entries { get; } = null!;
}

public class UserQueueConfig : IEntityTypeConfiguration<PlaybackQueue>
{
    public void Configure(EntityTypeBuilder<PlaybackQueue> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasMany(e => e.Entries).WithOne(e => e.Queue);
        builder.Property(e => e.DeviceId).HasMaxLength(256);
        builder.HasOne(e => e.Device).WithOne(e => e.PlaybackQueue);
    }
}
