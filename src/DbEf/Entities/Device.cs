using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Device : Entity
{
    public required string ConnectionId { get; set; } = null!;
    public required EntityId UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required string Name { get; set; } = null!;
    public PlaybackQueue PlaybackQueue { get; } = null!;
}

public sealed class DeviceConfig : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasIndex(e => e.ConnectionId).IsUnique();
        builder.Property(e => e.ConnectionId).HasMaxLength(256);
        builder.Property(e => e.Name).HasMaxLength(256);
    }
}
