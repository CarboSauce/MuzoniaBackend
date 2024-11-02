using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Muzonia.DbEf.Entities;

namespace Muzonia.DbEf;

public class UlidToBytesConverter : ValueConverter<Ulid, byte[]>
{
    private static readonly ConverterMappingHints defaultHints =
        new ConverterMappingHints(size: 16);

    public UlidToBytesConverter()
        : this(null!) { }

    public UlidToBytesConverter(ConverterMappingHints mappingHints = null!)
        : base(
            convertToProviderExpression: x => x.ToByteArray(),
            convertFromProviderExpression: x => new Ulid(x),
            mappingHints: defaultHints.With(mappingHints)
        ) { }
}

public class ApiDbContext(DbContextOptions<ApiDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Ulid>, Ulid>(options)
{
    public DbSet<Album> Albums { get; set; } = null!;
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<Song> Songs { get; set; } = null!;
    public DbSet<SongArtist> SongArtists { get; set; } = null!;
    public DbSet<ArtistAlbum> ArtistAlbums { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApiDbContext).Assembly);
    }

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder
    )
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<Ulid>()
            .HaveConversion<UlidToBytesConverter>();
    }
}
