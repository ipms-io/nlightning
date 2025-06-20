using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Exceptions;

/// <summary>
/// Represents an exception thrown when an error occurs.
/// </summary>
/// <remarks>
/// This exception is the base class for all exceptions that are thrown when an error occurs.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ErrorException : Exception
{
    public ErrorException(string message) : base(message) { }
    public ErrorException(string message, Exception innerException) : base(message, innerException) { }
}