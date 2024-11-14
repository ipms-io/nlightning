namespace NLightning.Bolts.Factories;

using BOLT1.Messages;
using BOLT1.Payloads;
using BOLT2.Messages;
using BOLT2.Payloads;
using Common.BitUtils;
using Common.TLVs;
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

    /// <summary>
    /// Create a TxRemoveInput message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <returns>The TxRemoveInput message.</returns>
    /// <seealso cref="TxRemoveInputMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxRemoveInputPayload"/>
    public static IMessage CreateTxRemoveInputMessage(ChannelId channelId, ulong serialId)
    {
        var payload = new TxRemoveInputPayload(channelId, serialId);

        return new TxRemoveInputMessage(payload);
    }

    /// <summary>
    /// Create a TxRemoveOutput message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="serialId">The serial id.</param>
    /// <returns>The TxRemoveOutput message.</returns>
    /// <seealso cref="TxRemoveOutputMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxRemoveOutputPayload"/>
    public static IMessage CreateTxRemoveOutputMessage(ChannelId channelId, ulong serialId)
    {
        var payload = new TxRemoveOutputPayload(channelId, serialId);

        return new TxRemoveOutputMessage(payload);
    }

    /// <summary>
    /// Create a TxComplete message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <returns>The TxComplete message.</returns>
    /// <seealso cref="TxCompleteMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxCompletePayload"/>
    public static IMessage CreateTxCompleteMessage(ChannelId channelId)
    {
        var payload = new TxCompletePayload(channelId);

        return new TxCompleteMessage(payload);
    }

    /// <summary>
    /// Create a TxSignatures message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="txId">The transaction id.</param>
    /// <param name="witnesses">The witnesses.</param>
    /// <returns>The TxSignatures message.</returns>
    /// <seealso cref="TxSignaturesMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxSignaturesPayload"/>
    public static IMessage CreateTxSignaturesMessage(ChannelId channelId, byte[] txId, List<Witness> witnesses)
    {
        var payload = new TxSignaturesPayload(channelId, txId, witnesses);

        return new TxSignaturesMessage(payload);
    }

    /// <summary>
    /// Create a TxInitRbf message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="locktime">The locktime.</param>
    /// <param name="feerate">The feerate</param>
    /// <param name="fundingOutputContrubution">The output contribution.</param>
    /// <param name="requireConfirmedInputs">How many confirmed inputs we need.</param>
    /// <returns>The TxInitRbf message.</returns>
    /// <seealso cref="TxInitRbfMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxInitRbfPayload"/>
    public static IMessage CreateTxInitRbfMessage(ChannelId channelId, uint locktime, uint feerate, long fundingOutputContrubution, bool requireConfirmedInputs)
    {
        var extension = new TlvStream();
        extension.Add(new FundingOutputContributionTlv(fundingOutputContrubution));
        if (requireConfirmedInputs)
        {
            extension.Add(new RequiredConfirmedInputsTlv());
        }

        var payload = new TxInitRbfPayload(channelId, locktime, feerate);

        return new TxInitRbfMessage(payload, extension);
    }

    /// <summary>
    /// Create a TxAckRbf message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="fundingOutputContrubution">The output contribution.</param>
    /// <param name="requireConfirmedInputs">How many confirmed inputs we need.</param>
    /// <returns>The TxAckRbf message.</returns>
    /// <seealso cref="TxAckRbfMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxAckRbfPayload"/>
    /// <seealso cref="TlvStream"/>
    /// <seealso cref="FundingOutputContributionTlv"/>
    /// <seealso cref="RequiredConfirmedInputsTlv"/>
    public static IMessage CreateTxAckRbfMessage(ChannelId channelId, long fundingOutputContrubution, bool requireConfirmedInputs)
    {
        var extension = new TlvStream();
        extension.Add(new FundingOutputContributionTlv(fundingOutputContrubution));
        if (requireConfirmedInputs)
        {
            extension.Add(new RequiredConfirmedInputsTlv());
        }

        var payload = new TxAckRbfPayload(channelId);

        return new TxAckRbfMessage(payload, extension);
    }

    /// <summary>
    /// Create a TxAbort message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="data">The data.</param>
    /// <returns>The TxAbort message.</returns>
    /// <seealso cref="TxAbortMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="TxAbortPayload"/>
    public static IMessage CreateTxAbortMessage(ChannelId channelId, byte[] data)
    {
        var payload = new TxAbortPayload(channelId, data);

        return new TxAbortMessage(payload);
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
            MessageTypes.WARNING => await WarningMessage.DeserializeAsync(stream),                  // 01 -> 0x01
            MessageTypes.INIT => await InitMessage.DeserializeAsync(stream),                        // 16 -> 0x10
            MessageTypes.ERROR => await ErrorMessage.DeserializeAsync(stream),                      // 17 -> 0x11
            MessageTypes.PING => await PingMessage.DeserializeAsync(stream),                        // 18 -> 0x12
            MessageTypes.PONG => await PongMessage.DeserializeAsync(stream),                        // 19 -> 0x13
            MessageTypes.TX_ADD_INPUT => await TxAddInputMessage.DeserializeAsync(stream),          // 66 -> 0x42
            MessageTypes.TX_ADD_OUTPUT => await TxAddOutputMessage.DeserializeAsync(stream),        // 67 -> 0x43
            MessageTypes.TX_REMOVE_INPUT => await TxRemoveInputMessage.DeserializeAsync(stream),    // 68 -> 0x44
            MessageTypes.TX_REMOVE_OUTPUT => await TxRemoveOutputMessage.DeserializeAsync(stream),  // 69 -> 0x45
            MessageTypes.TX_COMPLETE => await TxCompleteMessage.DeserializeAsync(stream),           // 70 -> 0x46
            MessageTypes.TX_SIGNATURES => await TxSignaturesMessage.DeserializeAsync(stream),       // 71 -> 0x47
            MessageTypes.TX_INIT_RBF => await TxInitRbfMessage.DeserializeAsync(stream),            // 72 -> 0x48
            MessageTypes.TX_ACK_RBF => await TxAckRbfMessage.DeserializeAsync(stream),              // 73 -> 0x49
            MessageTypes.TX_ABORT => await TxAbortMessage.DeserializeAsync(stream),                 // 74 -> 0x4A

            _ => throw new InvalidMessageException("Unknown message type"),
        };
    }
}