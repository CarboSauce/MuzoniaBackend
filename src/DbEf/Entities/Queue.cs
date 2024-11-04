using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class Queue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public required Guid SongId { get; set; }
    public Track Song { get; set; } = null!;
    public required long Index { get; set; }
}

public class QueueConfig : IEntityTypeConfiguration<Queue>
{
    public void Configure(EntityTypeBuilder<Queue> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User).WithMany();
        builder.HasOne(e => e.Song).WithMany();
    }
}
