namespace NLightning.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a warning occurs.
/// </summary>
/// <remarks>
/// A warning exception is an exception that should be caught and should not terminate the application.
/// </remarks>
public class WarningException(string message) : Exception(message)
{
}