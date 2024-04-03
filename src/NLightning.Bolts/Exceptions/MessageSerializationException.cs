namespace NLightning.Bolts.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs during message serialization or deserialization.
/// </summary>
public class MessageSerializationException : ErrorException
{
    public MessageSerializationException(string message) : base(message)
    { }
    public MessageSerializationException(string message, Exception innerException) : base(message, innerException)
    { }
}