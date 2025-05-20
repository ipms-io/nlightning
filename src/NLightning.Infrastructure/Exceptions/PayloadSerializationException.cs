using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Exceptions;

using Domain.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs during payload serialization or deserialization.
/// </summary>
[ExcludeFromCodeCoverage]
public class PayloadSerializationException : ErrorException
{
    public PayloadSerializationException(string message) : base(message)
    { }
    public PayloadSerializationException(string message, Exception innerException) : base(message, innerException)
    { }
}