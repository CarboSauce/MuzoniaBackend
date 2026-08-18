namespace Muzonia.Api.Services;

public class FunctionsClient(HttpClient httpClient)
{
    public HttpClient HttpClient { get; } = httpClient;
}
