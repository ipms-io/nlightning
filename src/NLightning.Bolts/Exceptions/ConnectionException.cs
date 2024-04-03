namespace NLightning.Bolts.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a connection error occurs.
/// </summary>
/// <remarks>
/// We usually want to close the connection when this exception is thrown.
/// </remarks>
public class ConnectionException : ErrorException
{
    public ConnectionException(string message) : base(message) { }
    public ConnectionException(string message, Exception innerException) : base(message, innerException) { }
}