namespace NLightning.Common.Exceptions;

/// <summary>
/// Represents a critical exception that is thrown when a critical error occurs.
/// </summary>
/// <remarks>
/// A critical exception is an exception that should not be caught and should terminate the application.
/// </remarks>
public class CriticalException : Exception
{
    public CriticalException(string message) : base(message) { }
    public CriticalException(string message, Exception innerException) : base(message, innerException) { }
}