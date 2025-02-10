using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class CurrentQueue
{
    public EntityId QueueId { get; set; }
    public PlaybackQueue Queue { get; } = null!;
    public EntityId UserId { get; set; }
    public AppUser User { get; } = null!;
}

public class CurrentQueueConfig : IEntityTypeConfiguration<CurrentQueue>
{
    public void Configure(EntityTypeBuilder<CurrentQueue> builder)
    {
        builder.HasKey(e => e.UserId);
        builder.HasOne(e => e.Queue).WithMany();
        builder.HasOne(e => e.User).WithOne();
    }
}
