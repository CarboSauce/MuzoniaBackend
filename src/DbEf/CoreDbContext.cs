using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Muzonia.DbEf.Entities;

namespace Muzonia.DbEf;

public class ApiDbContext(DbContextOptions<ApiDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Album> Albums { get; set; } = null!;
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<Track> Songs { get; set; } = null!;
    public DbSet<TrackArtist> SongArtists { get; set; } = null!;
    public DbSet<ArtistAlbum> ArtistAlbums { get; set; } = null!;
    public DbSet<Entities.FileModel> Files { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        IdentityRole<Guid>[] roles =
        [
            new()
            {
                Id = Guid.NewGuid(),
                ConcurrencyStamp = "1",
                Name = "Admin",
                NormalizedName = "ADMIN",
            },
            new()
            {
                Id = Guid.NewGuid(),
                ConcurrencyStamp = "2",
                Name = "User",
                NormalizedName = "USER",
            },
        ];

        builder.Entity<IdentityRole<Guid>>().HasData(roles);

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
            .Properties<Guid>()
            .HaveConversion<GuidToStringConverter>();
    }
}
