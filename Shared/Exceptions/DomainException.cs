namespace Shared.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with id '{key}' was not found")
    {
    }
}

public class ValidationException(string message, IEnumerable<string> errors) : DomainException(message)
{
    public IEnumerable<string> Errors { get; } = errors;

    public ValidationException(IEnumerable<string> errors) : this("Validation failed", errors)
    {
    }
}

public class BusinessRuleException(string message) : DomainException(message);

public class BusinessException(string message, IEnumerable<string> errors) : DomainException(message)
{
    public IEnumerable<string> Errors { get; } = errors;
}