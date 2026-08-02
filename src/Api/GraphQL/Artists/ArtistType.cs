using GreenDonut.Data;
using HotChocolate.Execution;
using Muzonia.Api.GraphQL.Tracks;
using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Artists;

[ObjectType<Artist>]
public static partial class ArtistType
{
    [UsePaging]
    [UseSorting]
    [BindMember(nameof(Artist.Tracks))]
    public static async Task<Page<Track>> GetTrackAsync(
        [Parent] Artist artist,
        ITracksByArtistIdDataLoader tracksByArtistIdDataLoader,
        PagingArguments paging,
        QueryContext<Track> queryContext,
        CancellationToken cancellationToken
    ) =>
        await tracksByArtistIdDataLoader
            .With(paging, queryContext)
            .LoadRequiredAsync(artist.Id, cancellationToken);
}
