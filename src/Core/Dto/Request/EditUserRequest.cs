using Microsoft.AspNetCore.Http;

namespace Muzonia.Core.Dto.Request;

public record EditUserRequest(string? UserName, string? Email, IFormFile? File);
