namespace Shared.Contracts.Validation;

/// <summary>
/// Marks a property as required (cannot be null or empty)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RequiredAttribute : Attribute
{
    /// <summary>
    /// Custom error message
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Marks a property as nullable (can be null)
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class NullableAttribute : Attribute
{
}

