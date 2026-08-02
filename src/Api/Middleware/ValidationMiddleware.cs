using System.ComponentModel.DataAnnotations;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using HotChocolate.Resolvers;

namespace Muzonia.Api.Middleware;

public class ValidationMiddleware(FieldDelegate next)
{
    public async Task InvokeAsync(IMiddlewareContext context)
    {
        foreach (var arg in context.Selection.Field.Arguments)
        {
            var value = context.ArgumentValue<object?>(arg.Name);

            if (value is null)
            {
                continue;
            }

            var validationContext = new ValidationContext(value);

            var results = new List<ValidationResult>();

            if (
                !Validator.TryValidateObject(
                    value,
                    validationContext,
                    results,
                    true
                )
            )
            {
                throw new GraphQLException(
                    ErrorBuilder
                        .New()
                        .SetMessage("Validation failed.")
                        .SetCode("VALIDATION_ERROR")
                        .SetExtension(
                            "errors",
                            results.Select(r => new
                            {
                                r.ErrorMessage,
                                Members = r.MemberNames,
                            })
                        )
                        .Build()
                );
            }
        }

        await next(context);
    }
}
