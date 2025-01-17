using Microsoft.EntityFrameworkCore;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.Common;

public static class ArtistUtils
{
    public static IQueryable<Artist?> GetArtist(
        this DbSet<AppUser> dbset,
        EntityId userId
    )
    {
        return dbset.Where(e => e.Id == userId).Select(e => e.Artist);
    }
}
