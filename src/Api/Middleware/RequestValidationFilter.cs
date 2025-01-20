using FluentValidation;
using Serilog.Core;

namespace Muzonia.Api.Middleware;

public class RequestValidationAsyncFilter<T>(
    ILogger<RequestValidationAsyncFilter<T>> logger,
    IValidator<T>? validator = null
) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var reqName = typeof(T).FullName;
        var req = context.Arguments.OfType<T>().First();

        if (validator is null)
        {
            return await next(context);
        }

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
