using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public record UserRequest(string? UserName, string? Email, IFormFile? File);
