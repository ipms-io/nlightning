namespace NLightning.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs.
/// </summary>
/// <remarks>
/// This exception is the base class for all exceptions that are thrown when an error occurs.
/// </remarks>
public class ErrorException : Exception
{
    public ErrorException(string message) : base(message) { }
    public ErrorException(string message, Exception innerException) : base(message, innerException) { }
}