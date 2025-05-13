namespace NLightning.Domain.Protocol.Interfaces;

using Models;

/// <summary>
/// Interface for a message.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// The type of the message. <see cref="Constants.MessageTypes"/>.
    /// </summary>
    ushort Type { get; }

    /// <summary>
    /// The payload of the message.
    /// </summary>
    IMessagePayload Payload { get; }

    /// <summary>
    /// The extension of the message, if any.
    /// </summary>
    TlvStream? Extension { get; }
}