using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class QueueUser : Entity
{
    public required EntityId UserId { get; set; }
    public required EntityId QueueId { get; set; }

    public required bool IsBanned { get; set; }
    public AppUser User { get; } = null!;
    public PlaybackQueue Queue { get; } = null!;
}

public class QueueUsersConfig : IEntityTypeConfiguration<QueueUser>
{
    public void Configure(EntityTypeBuilder<QueueUser> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.Queue).WithMany(e => e.QueueUsers);
        builder.HasOne(e => e.User).WithOne();
    }
}
