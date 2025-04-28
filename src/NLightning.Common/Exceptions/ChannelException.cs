using System.Diagnostics.CodeAnalysis;

namespace NLightning.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a channel error occurs.
/// </summary>
/// <remarks>
/// We usually want to close the connection when this exception is thrown.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ChannelException : ErrorException
{
    public ChannelException(string message) : base(message) { }
    public ChannelException(string message, Exception innerException) : base(message, innerException) { }
}