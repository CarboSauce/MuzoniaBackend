using Muzonia.DbEf.Entities;

namespace Muzonia.Api.GraphQL.Query;

public class TrackType : ObjectType<Track>
{
    protected override void Configure(IObjectTypeDescriptor<Track> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(x => x.Id);
        descriptor.Field(x => x.Duration);
    }
}
