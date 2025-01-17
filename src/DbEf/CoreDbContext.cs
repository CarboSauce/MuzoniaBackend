using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Muzonia.DbEf.Entities;

namespace Muzonia.DbEf;

public class ApiDbContext(DbContextOptions<ApiDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<EntityId>, EntityId>(options)
{
    public DbSet<Album> Albums { get; set; } = null!;
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<Track> Tracks { get; set; } = null!;
    public DbSet<TrackArtist> TrackArtists { get; set; } = null!;
    public DbSet<ArtistAlbum> ArtistAlbums { get; set; } = null!;
    public DbSet<FileModel> Files { get; set; } = null!;
    public DbSet<Playlist> Playlists { get; set; } = null!;
    public DbSet<PlaylistTrack> PlaylistTracks { get; set; } = null!;
    public DbSet<QueueEntry> QueueEntries { get; set; } = null!;
    public DbSet<PlaybackQueue> PlaybackQueues { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        var adminId = EntityId.Parse("85deccaa-119d-4d43-abbc-c92f76bc22be");
        var userId = EntityId.Parse("d4dd6c74-668f-4bd1-a74d-e6ad01175e76");

        IdentityRole<EntityId>[] roles =
        [
            new()
            {
                Id = adminId,
                ConcurrencyStamp = "1",
                Name = "Admin",
                NormalizedName = "ADMIN",
            },
            new()
            {
                Id = userId,
                ConcurrencyStamp = "2",
                Name = "User",
                NormalizedName = "USER",
            },
        ];

        builder.Entity<IdentityRole<EntityId>>().HasData(roles);

        builder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder
    )
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseExceptionProcessor();
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder
    )
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<EntityId>()
            .HaveConversion<GuidToStringConverter>();
    }
}
