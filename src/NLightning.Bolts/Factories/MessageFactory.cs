namespace NLightning.Bolts.Factories;

using BOLT1.Messages;
using BOLT1.Payloads;
using Common.BitUtils;
using Constants;
using Exceptions;
using Interfaces;

/// <summary>
/// Factory for creating messages.
/// </summary>
public static class MessageFactory
{
    #region Init Message
    /// <summary>
    /// Create an Init message.
    /// </summary>
    /// <param name="options">The node options.</param>
    /// <returns>The Init message.</returns>
    /// <seealso cref="InitMessage"/>
    /// <seealso cref="NodeOptions"/>
    /// <seealso cref="InitPayload"/>
    public static IMessage CreateInitMessage(NodeOptions options)
    {
        // Get features from options
        var features = options.GetNodeFeatures();
        var payload = new InitPayload(features);

        // Add default extension for Init message from options
        var extension = options.GetInitExtension();

        return new InitMessage(payload, extension);
    }
    #endregion

    #region Control Messages
    /// <summary>
    /// Create a Ping message.
    /// </summary>
    /// <returns>The Ping message.</returns>
    /// <seealso cref="PingMessage"/>
    /// <seealso cref="PingPayload"/>
    public static IMessage CreatePingMessage()
    {
        return new PingMessage();
    }

    /// <summary>
    /// Create a Pong message.
    /// </summary>
    /// <param name="bytesLen">The number of bytes in the pong payload.</param>
    /// <returns>The Pong message.</returns>
    /// <seealso cref="PongMessage"/>
    /// <seealso cref="PongPayload"/>
    public static IMessage CreatePongMessage(ushort bytesLen)
    {
        return new PongMessage(bytesLen);
    }
    #endregion

    /// <summary>
    /// Deserialize a message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized message.</returns>
    /// <exception cref="InvalidMessageException">Unknown message type</exception>
    public static async Task<IMessage> DeserializeMessageAsync(MemoryStream stream)
    {
        // Get type of message
        var typeBytes = new byte[2];
        await stream.ReadExactlyAsync(typeBytes);
        var type = EndianBitConverter.ToUInt16BigEndian(typeBytes);

        // Deserialize message based on type
        return type switch
        {
            MessageTypes.WARNING => await WarningMessage.DeserializeAsync(stream),
            MessageTypes.INIT => await InitMessage.DeserializeAsync(stream),
            MessageTypes.ERROR => await ErrorMessage.DeserializeAsync(stream),
            MessageTypes.PING => await PingMessage.DeserializeAsync(stream),
            MessageTypes.PONG => await PongMessage.DeserializeAsync(stream),

            _ => throw new InvalidMessageException("Unknown message type"),
        };
    }
}