using FluentValidation;
using Serilog.Core;

namespace Muzonia.Api.Middleware;

public class RequestValidationAsyncFilter<T>(
    IValidator<T> validator,
    ILogger<RequestValidationAsyncFilter<T>> logger
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var reqName = typeof(T).FullName;
        var req = context.Arguments.OfType<T>().First();

        logger.LogDebug("Validating {RequestName}", reqName);
        var result = await validator.ValidateAsync(
            req,
            context.HttpContext.RequestAborted
        );

        if (!result.IsValid)
        {
            logger.LogWarning(
                "Validation failed for {RequestName}: {ValidationErrors}",
                reqName,
                result.Errors
            );
            return TypedResults.ValidationProblem(result.ToDictionary());
        }

        logger.LogInformation("Validation passed for {RequestName}", reqName);
        return await next(context);
    }
}

// public class RequestValidationFilter<T>(
//     IValidator<T> validator,
//     ILogger<RequestValidationFilter<T>> logger
// ) : IEndpointFilter
// {
//     public ValueTask<object?> InvokeAsync(
//         EndpointFilterInvocationContext context,
//         EndpointFilterDelegate next
//     )
//     {
//         var reqName = typeof(T).FullName;
//
//         var req = context.Arguments.OfType<T>().First();
//
//         logger.LogDebug("Validating {RequestName}", reqName);
//         var result = validator.Validate(req);
//
//         if (!result.IsValid)
//         {
//             logger.LogWarning(
//                 "Validation failed for {RequestName}: {ValidationErrors}",
//                 reqName,
//                 result.Errors
//             );
//             return ValueTask.FromResult<object?>(
//                 TypedResults.ValidationProblem(result.ToDictionary())
//             );
//         }
//
//         logger.LogInformation("Validation passed for {RequestName}", reqName);
//         return next(context);
//     }
// }
