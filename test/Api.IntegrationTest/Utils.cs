using System.Diagnostics.CodeAnalysis;
using HotChocolate.Execution;
using HotChocolate.Transport.Http;
using OperationRequest = HotChocolate.Transport.OperationRequest;

namespace Muzonia.Api.IntegrationTest;

public static class Utils
{
    public static bool IsGuid(this string value) => Guid.TryParse(value, out _);

    extension(GraphQLHttpClient client)
    {
        public Task<GraphQLHttpResponse> ExecuteAsync(
            [StringSyntax("graphql")] string body,
            CancellationToken ct = default
        )
        {
            return client.PostAsync(new OperationRequest(body), ct);
        }
    }
}
