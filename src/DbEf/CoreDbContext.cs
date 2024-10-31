using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Muzonia.DbEf;

public class CoreDbContext(DbContextOptions<CoreDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Ulid>, Ulid>(options);
