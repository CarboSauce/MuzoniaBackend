using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Muzonia.Core.Common;

public class CommaSeparatedGuidArrayBinder<T> : IModelBinder
    where T : IParsable<T>
{
    private static T PrivateParse(string str) => T.Parse(str, null);

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(
            bindingContext.ModelName
        );

        if (valueProviderResult == ValueProviderResult.None)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var value = valueProviderResult.FirstValue;

        if (string.IsNullOrEmpty(value))
        {
            bindingContext.Result = ModelBindingResult.Success(
                Array.Empty<T>()
            );
            return Task.CompletedTask;
        }

        try
        {
            var guids = value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(PrivateParse)
                .ToArray();

            bindingContext.Result = ModelBindingResult.Success(guids);
        }
        catch
        {
            bindingContext.ModelState.AddModelError(
                bindingContext.ModelName,
                "Invalid array format."
            );
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}
