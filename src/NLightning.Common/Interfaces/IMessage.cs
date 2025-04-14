namespace NLightning.Common.Interfaces;

using Types;

/// <summary>
/// Interface for a message.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// The type of the message. <see cref="Common.Constants.MessageTypes"/>.
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

    /// <summary>
    /// Serialize the message to a stream.
    /// </summary>
    /// <param name="stream">The stream to serialize to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SerializeAsync(Stream stream);
}