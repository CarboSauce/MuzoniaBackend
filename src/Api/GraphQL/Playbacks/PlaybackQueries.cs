using System.Security.Claims;
using GreenDonut.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Muzonia.Api.Common;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Playbacks;

[QueryType]
public static partial class PlaybackQueries
{
    public static async Task<PlaybackQueue?> GetPlaybackQueue(
        ApiDbContext db,
        QueryContext<PlaybackQueue>? query,
        ClaimsPrincipal user,
        CancellationToken ct
    )
    {
        var userId = user.UserId;

        return await db
            .PlaybackQueues.Where(pq => pq.OwnerId == userId)
            .With(query)
            .SingleOrDefaultAsync(ct);
    }
}
