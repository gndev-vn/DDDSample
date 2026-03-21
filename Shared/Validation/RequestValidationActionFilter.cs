using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Exceptions;

namespace Shared.Validation;

public sealed class RequestValidationActionFilter(IServiceProvider serviceProvider) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<string>();

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (serviceProvider.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var validationResult = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .Where(message => !string.IsNullOrWhiteSpace(message)));
            }
        }

        if (errors.Count > 0)
        {
            throw new Shared.Exceptions.ValidationException(errors.Distinct().ToArray());
        }

        await next();
    }
}
