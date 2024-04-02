namespace NLightning.Bolts.Exceptions;

public class ConnectionTimeoutException : ErrorException
{
    public ConnectionTimeoutException(string message) : base(message) { }
    public ConnectionTimeoutException(string message, Exception innerException) : base(message, innerException) { }
}