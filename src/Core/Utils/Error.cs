using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Muzonia.Core.Utils;

public record Error(HttpStatusCode Code, string? Message) { }
