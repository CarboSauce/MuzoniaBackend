using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public class EditUserRequest
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public IFormFile? File { get; set; }
}
