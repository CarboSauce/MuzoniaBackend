using Microsoft.AspNetCore.Http;

namespace Muzonia.Api.IntegrationTest;

public static class Utils
{
    public static bool IsGuid(this string value) => Guid.TryParse(value, out _);

    public static FormFile DummyFormFile()
    {
        var content = "dummy";
        var fileName = "dummy";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        var formFile = new FormFile(ms, 0, ms.Length, "dummy", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };

        return formFile;
    }
}
