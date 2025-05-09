using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a warning occurs.
/// </summary>
/// <remarks>
/// A warning exception is an exception that should be caught and should not terminate the application.
/// </remarks>
[ExcludeFromCodeCoverage]
public class WarningException : Exception
{
    public WarningException(string message) : base(message) { }
    public WarningException(string message, Exception innerException) : base(message, innerException) { }
}