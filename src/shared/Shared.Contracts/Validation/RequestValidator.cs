using System.Reflection;

namespace Shared.Contracts.Validation;

/// <summary>
/// Simple validator that only checks Required attributes
/// </summary>
public static class RequestValidator
{
    /// <summary>
    /// Validates an object based on Required attributes
    /// Returns null if valid, otherwise returns validation errors dictionary
    /// </summary>
    public static Dictionary<string, string[]>? Validate(object? request)
    {
        if (request == null)
        {
            return new Dictionary<string, string[]>
                {
                    { "request", new[] { "Request body is required" } }
                };
        }

        // Use List<string> internally
        var errors = new Dictionary<string, List<string>>();
        var properties = request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
            // Note: NullableAttribute is a compiler-internal attribute and not reliable for runtime checks.
            var value = property.GetValue(request);

            if (requiredAttr != null)
            {
                if (IsNullOrEmpty(value))
                {
                    var fieldName = property.Name;
                    var errorMessage = requiredAttr.ErrorMessage ?? $"{fieldName} is required";

                    if (!errors.ContainsKey(fieldName))
                    {
                        errors[fieldName] = new List<string>();
                    }
                    errors[fieldName].Add(errorMessage);
                }
            }
        }

        if (errors.Count == 0)
            return null;

        // Convert List<string> to string[] for the return type
        var result = new Dictionary<string, string[]>();
        foreach (var kvp in errors)
        {
            result[kvp.Key] = kvp.Value.ToArray();
        }

        return result;
    }

    /// <summary>
    /// Checks if a value is null or empty
    /// </summary>
    private static bool IsNullOrEmpty(object? value)
    {
        if (value == null)
            return true;

        if (value is string str)
            return string.IsNullOrWhiteSpace(str);

        if (value is Guid guid)
            return guid == Guid.Empty;

        if (value is System.Collections.ICollection collection)
            return collection.Count == 0;

        return false;
    }

    /// <summary>
    /// Validates nested objects in a request
    /// </summary>
    public static Dictionary<string, string[]>? ValidateNested(object? request, string prefix = "")
    {
        if (request == null)
            return null;

        // Use List<string> internally
        var errors = new Dictionary<string, List<string>>();
        var properties = request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            var fieldName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";

            if (value != null && IsComplexType(property.PropertyType))
            {
                // Recurse
                var nested = ValidateNested(value, fieldName);
                if (nested != null)
                {
                    foreach (var kvp in nested)
                    {
                        // Merge nested errors
                        if (!errors.ContainsKey(kvp.Key))
                        {
                            errors[kvp.Key] = new List<string>();
                        }
                        errors[kvp.Key].AddRange(kvp.Value);
                    }
                }
            }
            else
            {
                var requiredAttr = property.GetCustomAttribute<RequiredAttribute>();
                if (requiredAttr != null && IsNullOrEmpty(value))
                {
                    var errorMessage = requiredAttr.ErrorMessage ?? $"{fieldName} is required";
                    if (!errors.ContainsKey(fieldName))
                    {
                        errors[fieldName] = new List<string>();
                    }
                    errors[fieldName].Add(errorMessage);
                }
            }
        }

        if (errors.Count == 0)
            return null;

        // Convert List<string> to string[] for the return type
        var result = new Dictionary<string, string[]>();
        foreach (var kvp in errors)
        {
            result[kvp.Key] = kvp.Value.ToArray();
        }

        return result;
    }

    /// <summary>
    /// Checks if a type is a complex type (not primitive)
    /// </summary>
    private static bool IsComplexType(Type type)
    {
        if (type.IsPrimitive || type.IsEnum)
            return false;

        if (type == typeof(string) ||
            type == typeof(DateTime) ||
            type == typeof(Guid) ||
            type == typeof(decimal) ||
            type == typeof(object))
            return false;

        // Treat collections as complex to allow nested validation if needed,
        // or handle specially elsewhere.
        return true;
    }
}

