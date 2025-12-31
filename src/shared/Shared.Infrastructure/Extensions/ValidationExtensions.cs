using FluentValidation;
using Shared.Infrastructure.Exceptions;

namespace Shared.Infrastructure.Extensions;

public static class ValidationExtensions
{
    /// <summary>
    /// Validates a request using FluentValidation and throws ValidationException if invalid
    /// </summary>
    public static async Task ValidateAndThrowAsync<T>(this IValidator<T> validator, T instance, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(instance, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            throw new ValidationException(errors);
        }
    }
}

