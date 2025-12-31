namespace Shared.Infrastructure.Exceptions;

/// <summary>
/// Base exception for application-specific exceptions
/// </summary>
public abstract class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }

    protected ApplicationException(string errorCode, string message, int statusCode = 400)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    protected ApplicationException(string errorCode, string message, Exception innerException, int statusCode = 400)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

/// <summary>
/// Exception for validation errors
/// </summary>
public class ValidationException : AppException
{
    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(Dictionary<string, string[]> validationErrors)
        : base(Shared.Contracts.Common.ErrorCodes.ValidationError, "One or more validation errors occurred", 400)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(string field, string error)
        : base(Shared.Contracts.Common.ErrorCodes.ValidationError, "One or more validation errors occurred", 400)
    {
        ValidationErrors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}

/// <summary>
/// Exception for not found errors
/// </summary>
public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object key)
        : base(Shared.Contracts.Common.ErrorCodes.NotFound, $"{entityName} with id '{key}' was not found", 404)
    {
    }

    public NotFoundException(string message)
        : base(Shared.Contracts.Common.ErrorCodes.NotFound, message, 404)
    {
    }
}

/// <summary>
/// Exception for conflict errors (e.g., duplicate entries)
/// </summary>
public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(Shared.Contracts.Common.ErrorCodes.Conflict, message, 409)
    {
    }
}

/// <summary>
/// Exception for unauthorized access
/// </summary>
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(Shared.Contracts.Common.ErrorCodes.Unauthorized, message, 401)
    {
    }
}

/// <summary>
/// Exception for forbidden access
/// </summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden access")
        : base(Shared.Contracts.Common.ErrorCodes.Forbidden, message, 403)
    {
    }
}

/// <summary>
/// Exception for business rule violations
/// </summary>
public class BusinessRuleException : AppException
{
    public BusinessRuleException(string errorCode, string message)
        : base(errorCode, message, 422)
    {
    }
}

