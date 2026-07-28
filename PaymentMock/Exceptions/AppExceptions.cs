namespace PaymentMock.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(string error) : base(error)
    {
        Errors = new List<string> { error };
    }

    public ValidationException(List<string> errors) : base(string.Join("; ", errors))
    {
        Errors = errors;
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

public class GatewayUnauthorizedException : Exception
{
    public GatewayUnauthorizedException(string message) : base(message) { }
}
