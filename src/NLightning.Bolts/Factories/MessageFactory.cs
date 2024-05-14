namespace NLightning.Bolts.Factories;

using BOLT1.Messages;
using BOLT1.Payloads;
using BOLT2.Messages;
using BOLT2.Payloads;
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

    #region Interactive Transaction Construction
    /// <summary>
    /// Create a TxAddInput message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <param name="prevTx">The previous transaction.</param>
    /// <param name="prevTxVout">The previous transaction vout.</param>
    /// <param name="sequence">The sequence number.</param>
    /// <returns>The TxAddInput message.</returns>
    /// <seealso cref="TxAddInputMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxAddInputPayload"/>
    public static IMessage CreateTxAddInputMessage(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout, uint sequence)
    {
        var payload = new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout, sequence);

        return new TxAddInputMessage(payload);
    }

    /// <summary>
    /// Create a TxAddOutput message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <param name="sats">The number of satoshis.</param>
    /// <param name="script">The script.</param>
    /// <returns>The TxAddOutput message.</returns>
    /// <seealso cref="TxAddOutputMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxAddOutputPayload"/>
    public static IMessage CreateTxAddOutputMessage(ChannelId channelId, ulong serialId, ulong sats, byte[] script)
    {
        var payload = new TxAddOutputPayload(channelId, serialId, sats, script);

        return new TxAddOutputMessage(payload);
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
            MessageTypes.WARNING => await WarningMessage.DeserializeAsync(stream),              // 01 -> 0x01
            MessageTypes.INIT => await InitMessage.DeserializeAsync(stream),                    // 16 -> 0x10
            MessageTypes.ERROR => await ErrorMessage.DeserializeAsync(stream),                  // 17 -> 0x11
            MessageTypes.PING => await PingMessage.DeserializeAsync(stream),                    // 18 -> 0x12
            MessageTypes.PONG => await PongMessage.DeserializeAsync(stream),                    // 19 -> 0x13
            MessageTypes.TX_ADD_INPUT => await TxAddInputMessage.DeserializeAsync(stream),      // 66 -> 0x42
            MessageTypes.TX_ADD_OUTPUT => await TxAddOutputMessage.DeserializeAsync(stream),    // 67 -> 0x43

            _ => throw new InvalidMessageException("Unknown message type"),
        };
    }
}