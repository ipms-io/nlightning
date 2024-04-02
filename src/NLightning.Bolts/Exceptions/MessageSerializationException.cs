namespace NLightning.Bolts.Exceptions;

public class MessageSerializationException : ErrorException
{
    public MessageSerializationException(string message) : base(message)
    { }
    public MessageSerializationException(string message, Exception innerException) : base(message, innerException)
    { }
}