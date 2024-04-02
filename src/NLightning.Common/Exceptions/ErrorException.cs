namespace NLightning.Common.Exceptions;

public class ErrorException : Exception
{
    public ErrorException(string message) : base(message) { }
    public ErrorException(string message, Exception innerException) : base(message, innerException) { }
}