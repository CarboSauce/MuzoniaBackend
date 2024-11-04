namespace Muzonia.Core.Exceptions;

public class ForbiddenException : Exception
{
    public object? Errors { get; }

    public ForbiddenException(string message)
        : base(message) { }

    public ForbiddenException(object? errors) => Errors = errors;
}
