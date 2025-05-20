using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Exceptions;

/// <summary>
/// Represents an exception thrown when an invalid message is received.
/// </summary>
/// <remarks>
/// This exception is thrown when an unknown message type is received
/// or when a message is received that is not expected.
/// </remarks>
[ExcludeFromCodeCoverage]
public class InvalidMessageException(string message) : Exception
{
    public new string Message = message;

    /// <summary>
    /// The expected type of the message.
    /// </summary>
    public object? ExpectedType;

    /// <summary>
    /// The received type of the message.
    /// </summary>
    public object? ReceivedType;

    /// <summary>
    /// Initializes a new instance of the InvalidMessageException class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="received">The received type of the message.</param>
    /// <param name="expected">The expected type of the message.</param>
    /// <remarks>
    /// If the message contains placeholders for types, the message will be formatted.
    /// Like this: "Received type {0} but expected type {1}"
    /// </remarks>
    public InvalidMessageException(string message, object received, object? expected = null) : this(message)
    {
        ReceivedType = received;
        ExpectedType = expected;

        // Check if the message has placeholder for types and format the string
        if (message.Contains("{0}"))
        {
            if (message.Contains("{1}"))
            {
                Message = string.Format(message, received, expected);
            }
            else
            {
                Message = string.Format(message, received);
            }
        }
        else
        {
            Message = message;
        }
    }
}