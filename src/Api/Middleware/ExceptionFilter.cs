using System;
using System.Net;
using System.Security.Authentication;
using EntityFramework.Exceptions.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Muzonia.Core.Exceptions;

namespace Muzonia.Api.Middleware;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
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
            _ => context.Result,
        };
    }
}

public static class ApiExceptionFilterExt
{
    private static HttpStatusCode TranslateCode(this Exception ex) =>
        ex switch
        {
            AuthenticationException => HttpStatusCode.Forbidden,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            BadRequestException => HttpStatusCode.BadRequest,
            NotFoundException => HttpStatusCode.NotFound,
            UniqueConstraintException => HttpStatusCode.Conflict,
            MaxLengthExceededException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError,
        };

    public static void AddExceptionFilter(this WebApplication app)
    {
        app.UseExceptionHandler(o =>
        {
            o.Run(async context =>
            {
                var ex = context.Features.Get<IExceptionHandlerFeature>();

                var statusCode =
                    ex?.Error.TranslateCode()
                    ?? HttpStatusCode.InternalServerError;

                await Results
                    .Problem(statusCode: (int)statusCode)
                    .ExecuteAsync(context);
            });
        });
    }
}
