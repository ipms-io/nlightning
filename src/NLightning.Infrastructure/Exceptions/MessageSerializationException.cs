using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Exceptions;

using Domain.Exceptions;

/// <summary>
/// Represents an exception thrown when an error occurs during message serialization or deserialization.
/// </summary>
[ExcludeFromCodeCoverage]
public class MessageSerializationException : ErrorException
{
    public MessageSerializationException(string message) : base(message)
    { }
    public MessageSerializationException(string message, Exception innerException) : base(message, innerException)
    { }
}