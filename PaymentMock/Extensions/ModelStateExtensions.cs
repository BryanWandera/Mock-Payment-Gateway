using Microsoft.AspNetCore.Mvc.ModelBinding;
using PaymentMock.Exceptions;

namespace PaymentMock.Extensions;

public static class ModelStateExtensions
{
    public static void ThrowIfInvalid(this ModelStateDictionary modelState)
    {
        if (modelState.IsValid)
            return;

        var errors = modelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        throw new ValidationException(errors);
    }
}
