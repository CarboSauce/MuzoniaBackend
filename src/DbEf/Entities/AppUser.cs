using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Muzonia.DbEf.Entities;

public sealed class AppUser : Entity
{
    public string Email { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public Profile Profile { get; set; }

    public AppUser()
    {
        CreationDate = DateTime.UtcNow;
        Id = EntityId.NewGuid();
    }
}

public class AppUserConfig : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(e => e.Email).HasColumnName("email");
        builder.Property(e => e.Name).HasColumnName("name");
        builder.Property(e => e.Role).HasColumnName("role");
        builder.Property(e => e.Id).HasColumnName("id");
    }
}
