using System;
using System.Security.Authentication;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Muzonia.Core.Exceptions;

namespace Muzonia.Api.Middleware;

public class ApiExceptionFilter : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        var exception = context.Exception;

        context.Result = exception switch
        {
            AuthenticationException => new ForbidResult(),
            UnauthorizedAccessException => new UnauthorizedResult(),
            BadRequestException => new BadRequestResult(),
            NotFoundException => new NotFoundResult(),
            UniqueConstraintException => new ConflictResult(),
            MaxLengthExceededException => new BadRequestResult(),
            _ => context.Result
        };
    }
}
