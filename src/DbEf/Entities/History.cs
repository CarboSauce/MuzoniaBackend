using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public class History : Entity
{
    public required EntityId UserId { get; set; }
    public AppUser User { get; } = null!;
    public required EntityId SongId { get; set; }
    public Track Song { get; } = null!;
}

public class HistoryConfig : IEntityTypeConfiguration<History>
{
    public void Configure(EntityTypeBuilder<History> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.User).WithMany();
        builder.HasOne(e => e.Song).WithMany();
    }
}
