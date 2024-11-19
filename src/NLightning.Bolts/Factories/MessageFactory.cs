using NBitcoin;
using NBitcoin.Crypto;

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

        return new InitMessage(payload, options.GetInitTlvs());
    }
    #endregion

    #region Control Messages
    /// <summary>
    /// Create a Warning message.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="channelId">The channel id, if any.</param>
    /// <returns>The Warning message.</returns>
    /// <seealso cref="WarningMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ErrorPayload"/>
    public static IMessage CreateWarningMessage(string message, ChannelId? channelId)
    {
        var payload = channelId is null ? new ErrorPayload(message) : new ErrorPayload(channelId.Value, message);
        return new WarningMessage(payload);
    }

    /// <summary>
    /// Create a Warning message.
    /// </summary>
    /// <param name="data">The data to be sent.</param>
    /// <param name="channelId">The channel id, if any.</param>
    /// <returns>The Warning message.</returns>
    /// <seealso cref="WarningMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ErrorPayload"/>
    public static IMessage CreateWarningMessage(byte[] data, ChannelId? channelId)
    {
        var payload = channelId is null ? new ErrorPayload(data) : new ErrorPayload(channelId.Value, data);
        return new WarningMessage(payload);
    }

    /// <summary>
    /// Create a Stfu message.
    /// </summary>
    /// <param name="channelId">The channel id, if any.</param>
    /// <param name="initiator">If we are the sender of the message.</param>
    /// <returns>The Stfu message.</returns>
    /// <seealso cref="StfuMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="StfuPayload"/>
    public static IMessage CreateStfuMessage(ChannelId channelId, bool initiator)
    {
        var payload = new StfuPayload(channelId, initiator);
        return new StfuMessage(payload);
    }

    /// <summary>
    /// Create a Error message.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="channelId">The channel id, if any.</param>
    /// <returns>The Error message.</returns>
    /// <seealso cref="ErrorMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ErrorPayload"/>
    public static IMessage CreateErrorMessage(string message, ChannelId? channelId)
    {
        var payload = channelId is null ? new ErrorPayload(message) : new ErrorPayload(channelId.Value, message);
        return new ErrorMessage(payload);
    }

    /// <summary>
    /// Create a Error message.
    /// </summary>
    /// <param name="data">The data to be sent.</param>
    /// <param name="channelId">The channel id, if any.</param>
    /// <returns>The Error message.</returns>
    /// <seealso cref="ErrorMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ErrorPayload"/>
    public static IMessage CreateErrorMessage(byte[] data, ChannelId? channelId)
    {
        var payload = channelId is null ? new ErrorPayload(data) : new ErrorPayload(channelId.Value, data);
        return new ErrorMessage(payload);
    }

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
    public static IMessage CreateTxAddInputMessage(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout,
                                                   uint sequence)
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
    public static IMessage CreateTxInitRbfMessage(ChannelId channelId, uint locktime, uint feerate,
                                                  long fundingOutputContrubution, bool requireConfirmedInputs)
    {
        FundingOutputContributionTlv? fundingOutputContrubutionTlv = null;
        RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null;

        if (fundingOutputContrubution > 0)
        {
            fundingOutputContrubutionTlv = new FundingOutputContributionTlv(fundingOutputContrubution);
        }

        if (requireConfirmedInputs)
        {
            requireConfirmedInputsTlv = new RequireConfirmedInputsTlv();
        }

        var payload = new TxInitRbfPayload(channelId, locktime, feerate);

        return new TxInitRbfMessage(payload, fundingOutputContrubutionTlv, requireConfirmedInputsTlv);
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
    /// <seealso cref="RequireConfirmedInputsTlv"/>
    public static IMessage CreateTxAckRbfMessage(ChannelId channelId, long fundingOutputContrubution,
                                                 bool requireConfirmedInputs)
    {
        FundingOutputContributionTlv? fundingOutputContrubutionTlv = null;
        RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null;

        if (fundingOutputContrubution > 0)
        {
            fundingOutputContrubutionTlv = new FundingOutputContributionTlv(fundingOutputContrubution);
        }

        if (requireConfirmedInputs)
        {
            requireConfirmedInputsTlv = new RequireConfirmedInputsTlv();
        }

        var payload = new TxAckRbfPayload(channelId);

        return new TxAckRbfMessage(payload, fundingOutputContrubutionTlv, requireConfirmedInputsTlv);
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

    #region Channel Messages
    /// <summary>
    /// Create a Shutdown message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="scriptPubkey">The Script to send closing funds to.</param>
    /// <returns>The Shutdown message.</returns>
    /// <seealso cref="ShutdownMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="Script"/>
    /// <seealso cref="ShutdownPayload"/>
    public static IMessage CreateShutdownMessage(ChannelId channelId, Script scriptPubkey)
    {
        var payload = new ShutdownPayload(channelId, scriptPubkey);

        return new ShutdownMessage(payload);
    }

    /// <summary>
    /// Create a ClosingSigned message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="feeSatoshis">The fee we want them to pay for closing the channel.</param>
    /// <param name="signature">The signature for closing the channel.</param>
    /// <param name="minFeeSatoshis">The min fee we will accept them to pay to close the channel.</param>
    /// <param name="maxFeeSatoshis">The max fee we will accept them to pay to close the channel.</param>
    /// <returns>The ClosingSigned message.</returns>
    /// <seealso cref="ClosingSignedMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ECDSASignature"/>
    /// <seealso cref="ClosingSignedPayload"/>
    public static IMessage CreateClosingSignedMessage(ChannelId channelId, ulong feeSatoshis, ECDSASignature signature,
                                                      ulong minFeeSatoshis, ulong maxFeeSatoshis)
    {
        var payload = new ClosingSignedPayload(channelId, feeSatoshis, signature);

        return new ClosingSignedMessage(payload, new FeeRangeTlv(minFeeSatoshis, maxFeeSatoshis));
    }

    /// <summary>
    /// Create a OpenChannel2 message.
    /// </summary>
    /// <param name="temporaryChannelId">The temporary channel id.</param>
    /// <param name="fundingFeeRatePerKw">The funding fee rate to open the channel.</param>
    /// <param name="commitmentFeeRatePerKw">The commitment fee rate.</param>
    /// <param name="fundingSatoshis">The amount of satoshis we're adding to the channel.</param>
    /// <param name="fundingPubKey">The funding pubkey of the channel.</param>
    /// <param name="revocationBasepoint">The revocation pubkey.</param>
    /// <param name="paymentBasepoint">The payment pubkey.</param>
    /// <param name="delayedPaymentBasepoint">The delayed payment pubkey.</param>
    /// <param name="htlcBasepoint">The htlc pubkey.</param>
    /// <param name="firstPerCommitmentPoint">The first per commitment pubkey.</param>
    /// <param name="secondPerCommitmentPoint">The second per commitment pubkey.</param>
    /// <param name="channelFlags">The flags for the channel.</param>
    /// <param name="shutdownScriptPubkey">The shutdown script to be used when closing the channel.</param>
    /// <param name="channelType">The type of the channel.</param>
    /// <param name="requireConfirmedInputs">If we want confirmed inputs to open the channel.</param>
    /// <returns>The OpenChannel2 message.</returns>
    /// <seealso cref="OpenChannel2Message"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="PubKey"/>
    /// <seealso cref="ChannelFlags"/>
    /// <seealso cref="Script"/>
    /// <seealso cref="OpenChannel2Payload"/>
    public static IMessage CreateOpenChannel2Message(ChannelId temporaryChannelId, uint fundingFeeRatePerKw,
                                                     uint commitmentFeeRatePerKw, ulong fundingSatoshis,
                                                     PubKey fundingPubKey, PubKey revocationBasepoint,
                                                     PubKey paymentBasepoint, PubKey delayedPaymentBasepoint,
                                                     PubKey htlcBasepoint, PubKey firstPerCommitmentPoint,
                                                     PubKey secondPerCommitmentPoint, ChannelFlags channelFlags,
                                                     Script? shutdownScriptPubkey = null, byte[]? channelType = null,
                                                     bool requireConfirmedInputs = false)
    {
        var payload = new OpenChannel2Payload(temporaryChannelId, fundingFeeRatePerKw, commitmentFeeRatePerKw,
                                              fundingSatoshis, fundingPubKey, revocationBasepoint, paymentBasepoint,
                                              delayedPaymentBasepoint, htlcBasepoint, firstPerCommitmentPoint,
                                              secondPerCommitmentPoint, channelFlags);

        return new OpenChannel2Message(payload,
                                       shutdownScriptPubkey is null
                                           ? null
                                           : new UpfrontShutdownScriptTlv(shutdownScriptPubkey),
                                       channelType is null ?
                                           null
                                           : new ChannelTypeTlv(channelType),
                                       requireConfirmedInputs ? new RequireConfirmedInputsTlv() : null);
    }

    /// <summary>
    /// Create a AcceptChannel2 message.
    /// </summary>
    /// <param name="temporaryChannelId">The temporary channel id.</param>
    /// <param name="fundingSatoshis">The amount of satoshis we're adding to the channel.</param>
    /// <param name="fundingPubKey">The funding pubkey of the channel.</param>
    /// <param name="revocationBasepoint">The revocation pubkey.</param>
    /// <param name="paymentBasepoint">The payment pubkey.</param>
    /// <param name="delayedPaymentBasepoint">The delayed payment pubkey.</param>
    /// <param name="htlcBasepoint">The htlc pubkey.</param>
    /// <param name="firstPerCommitmentPoint">The first per commitment pubkey.</param>
    /// <param name="shutdownScriptPubkey">The shutdown script to be used when closing the channel.</param>
    /// <param name="channelType">The type of the channel.</param>
    /// <param name="requireConfirmedInputs">If we want confirmed inputs to open the channel.</param>
    /// <returns>The AcceptChannel2 message.</returns>
    /// <seealso cref="AcceptChannel2Message"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="PubKey"/>
    /// <seealso cref="Script"/>
    /// <seealso cref="AcceptChannel2Payload"/>
    public static IMessage CreateAcceptChannel2Message(ChannelId temporaryChannelId, ulong fundingSatoshis,
                                                     PubKey fundingPubKey, PubKey revocationBasepoint,
                                                     PubKey paymentBasepoint, PubKey delayedPaymentBasepoint,
                                                     PubKey htlcBasepoint, PubKey firstPerCommitmentPoint,
                                                     Script? shutdownScriptPubkey = null, byte[]? channelType = null,
                                                     bool requireConfirmedInputs = false)
    {
        var payload = new AcceptChannel2Payload(temporaryChannelId, fundingSatoshis, fundingPubKey, revocationBasepoint,
                                                paymentBasepoint, delayedPaymentBasepoint, htlcBasepoint,
                                                firstPerCommitmentPoint);

        return new AcceptChannel2Message(payload,
                                       shutdownScriptPubkey is null
                                           ? null
                                           : new UpfrontShutdownScriptTlv(shutdownScriptPubkey),
                                       channelType is null ?
                                           null
                                           : new ChannelTypeTlv(channelType),
                                       requireConfirmedInputs ? new RequireConfirmedInputsTlv() : null);
    }
    #endregion

    #region Commitment
    /// <summary>
    /// Create a CommitmentSigned message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="signature">The signature for the commitment transaction.</param>
    /// <param name="htlcSignatures">The signatures for each open htlc.</param>
    /// <returns>The CommitmentSigned message.</returns>
    /// <seealso cref="CommitmentSignedMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ECDSASignature"/>
    /// <seealso cref="CommitmentSignedPayload"/>
    public static IMessage CreateCommitmentSignedMessage(ChannelId channelId, ECDSASignature signature, IEnumerable<ECDSASignature> htlcSignatures)
    {
        var payload = new CommitmentSignedPayload(channelId, signature, htlcSignatures);

        return new CommitmentSignedMessage(payload);
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
            MessageTypes.WARNING => await WarningMessage.DeserializeAsync(stream),                      // 01  -> 0x01
            MessageTypes.INIT => await InitMessage.DeserializeAsync(stream),                            // 16  -> 0x10
            MessageTypes.ERROR => await ErrorMessage.DeserializeAsync(stream),                          // 17  -> 0x11
            MessageTypes.PING => await PingMessage.DeserializeAsync(stream),                            // 18  -> 0x12
            MessageTypes.PONG => await PongMessage.DeserializeAsync(stream),                            // 19  -> 0x13
            MessageTypes.SHUTDOWN => await ShutdownMessage.DeserializeAsync(stream),                    // 38  -> 0x26
            MessageTypes.CLOSING_SIGNED => await ClosingSignedMessage.DeserializeAsync(stream),         // 39  -> 0x27
            MessageTypes.OPEN_CHANNEL_2 => await OpenChannel2Message.DeserializeAsync(stream),          // 64  -> 0x40
            MessageTypes.ACCEPT_CHANNEL_2 => await AcceptChannel2Message.DeserializeAsync(stream),      // 65  -> 0x41
            MessageTypes.TX_ADD_INPUT => await TxAddInputMessage.DeserializeAsync(stream),              // 66  -> 0x42
            MessageTypes.TX_ADD_OUTPUT => await TxAddOutputMessage.DeserializeAsync(stream),            // 67  -> 0x43
            MessageTypes.TX_REMOVE_INPUT => await TxRemoveInputMessage.DeserializeAsync(stream),        // 68  -> 0x44
            MessageTypes.TX_REMOVE_OUTPUT => await TxRemoveOutputMessage.DeserializeAsync(stream),      // 69  -> 0x45
            MessageTypes.TX_COMPLETE => await TxCompleteMessage.DeserializeAsync(stream),               // 70  -> 0x46
            MessageTypes.TX_SIGNATURES => await TxSignaturesMessage.DeserializeAsync(stream),           // 71  -> 0x47
            MessageTypes.TX_INIT_RBF => await TxInitRbfMessage.DeserializeAsync(stream),                // 72  -> 0x48
            MessageTypes.TX_ACK_RBF => await TxAckRbfMessage.DeserializeAsync(stream),                  // 73  -> 0x49
            MessageTypes.TX_ABORT => await TxAbortMessage.DeserializeAsync(stream),                     // 74  -> 0x4A
            MessageTypes.COMMITMENT_SIGNED => await CommitmentSignedMessage.DeserializeAsync(stream),   // 132 -> 0x84

            _ => throw new InvalidMessageException("Unknown message type"),
        };
    }
}