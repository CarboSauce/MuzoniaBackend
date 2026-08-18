using System.ComponentModel.DataAnnotations;
using FluentValidation;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using HotChocolate.Resolvers;

namespace Muzonia.Api.Middleware;

public class ValidationMiddleware(FieldDelegate next)
{
    public async Task InvokeAsync(
        IMiddlewareContext context,
        IServiceProvider serviceProvider
    )
    {
        foreach (var arg in context.Selection.Field.Arguments)
        {
            var value = context.ArgumentValue<object?>(arg.Name);

            if (value is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(
                value.GetType()
            );

            if (
                serviceProvider.GetService(validatorType)
                is not IValidator validator
            )
                continue;

            var validationContext = new ValidationContext<object>(value);

            var result = await validator.ValidateAsync(
                validationContext,
                context.RequestAborted
            );

            if (!result.IsValid)
            {
                var errors = result.Errors.Select(error =>
                    ErrorBuilder
                        .New()
                        .SetMessage(error.ErrorMessage)
                        .SetCode("VALIDATION_ERROR")
                        .SetPath(context.Path)
                        .Build()
                );

                throw new GraphQLException(errors);
            }
        }

        await next(context);
    }
}
