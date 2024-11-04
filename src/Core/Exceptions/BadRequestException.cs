namespace Muzonia.Core.Exceptions;

public class BadRequestException : Exception
{
    public object? Errors { get; }

    public BadRequestException(object errors)
    {
        Errors = errors;
    }

    public BadRequestException(string message)
        : base(message) { }
}

public class NotFoundException : Exception
{
    public object? Errors { get; }

    public NotFoundException(object errors)
    {
        Errors = errors;
    }

    public NotFoundException(string message)
        : base(message) { }
}
