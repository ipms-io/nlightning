namespace NLightning.Bolts.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a connection timeout occurs.
/// </summary>
public class ConnectionTimeoutException : ConnectionException
{
    public ConnectionTimeoutException(string message) : base(message) { }
    public ConnectionTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}