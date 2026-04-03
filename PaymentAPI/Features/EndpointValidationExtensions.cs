using FluentValidation;

namespace PaymentAPI.Features;

internal static class EndpointValidationExtensions
{
    public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T model, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(model, cancellationToken);
        if (validationResult.IsValid)
        {
            return;
        }

        var errors = validationResult.Errors
            .Select(error => error.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Distinct()
            .ToArray();

        throw new Shared.Exceptions.ValidationException(errors);
    }
}