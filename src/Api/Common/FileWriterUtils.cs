using Muzonia.Core.Services;

namespace Muzonia.Api.Common;

public static class FileWriterUtils
{
    public static Task<Uri?> WriteImage(
        this IFileWriter fileWriter,
        IFormFile file
    )
    {
        return fileWriter.WriteAsync(
            file,
            "images/",
            EntityId.NewGuid().ToString()
        );
    }
}
