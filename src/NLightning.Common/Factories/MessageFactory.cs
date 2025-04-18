using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Common.Factories;

using BitUtils;
using Constants;
using Exceptions;
using Interfaces;
using Messages;
using Messages.Payloads;
using Options;
using TLVs;
using Types;

/// <summary>
/// Factory for creating messages.
/// </summary>
public class MessageFactory : IMessageFactory
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
    public IMessage CreateInitMessage(NodeOptions options)
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
    public IMessage CreateWarningMessage(string message, ChannelId? channelId)
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
    public IMessage CreateWarningMessage(byte[] data, ChannelId? channelId)
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
    public IMessage CreateStfuMessage(ChannelId channelId, bool initiator)
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
    public IMessage CreateErrorMessage(string message, ChannelId? channelId)
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
    public IMessage CreateErrorMessage(byte[] data, ChannelId? channelId)
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
    public IMessage CreatePingMessage()
    {
        return new PingMessage();
    }

    /// <summary>
    /// Create a Pong message.
    /// </summary>
    /// <param name="pingMessage">The ping message we're responding to</param>
    /// <returns>The Pong message.</returns>
    /// <seealso cref="PongMessage"/>
    /// <seealso cref="PongPayload"/>
    public IMessage CreatePongMessage(IMessage pingMessage)
    {
        if (pingMessage is not PingMessage ping)
        {
            throw new ArgumentException("Ping message is required", nameof(pingMessage));
        }

        return new PongMessage(ping.Payload.NumPongBytes);
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
    public IMessage CreateTxAddInputMessage(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout,
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
    public IMessage CreateTxAddOutputMessage(ChannelId channelId, ulong serialId, ulong sats, byte[] script)
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
    public IMessage CreateTxRemoveInputMessage(ChannelId channelId, ulong serialId)
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
    public IMessage CreateTxRemoveOutputMessage(ChannelId channelId, ulong serialId)
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
    public IMessage CreateTxCompleteMessage(ChannelId channelId)
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
    public IMessage CreateTxSignaturesMessage(ChannelId channelId, byte[] txId, List<Witness> witnesses)
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
    public IMessage CreateTxInitRbfMessage(ChannelId channelId, uint locktime, uint feerate,
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
    public IMessage CreateTxAckRbfMessage(ChannelId channelId, long fundingOutputContrubution,
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
    public IMessage CreateTxAbortMessage(ChannelId channelId, byte[] data)
    {
        var payload = new TxAbortPayload(channelId, data);

        return new TxAbortMessage(payload);
    }
    #endregion

    #region Channel Messages
    /// <summary>
    /// Create a ChannelReady message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="secondPerCommitmentPoint">The second per commitment point.</param>
    /// <param name="shortChannelId">The channel's shortChannelId.</param>
    /// <returns>The ChannelReady message.</returns>
    /// <seealso cref="ChannelReadyMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="PubKey"/>
    /// <seealso cref="ShortChannelId"/>
    /// <seealso cref="ChannelReadyPayload"/>
    public IMessage CreateChannelReadyMessage(ChannelId channelId, PubKey secondPerCommitmentPoint,
                                                     ShortChannelId? shortChannelId = null)
    {
        var payload = new ChannelReadyPayload(channelId, secondPerCommitmentPoint);

        return new ChannelReadyMessage(payload,
                                       shortChannelId is null ? null : new ShortChannelIdTlv(shortChannelId.Value));
    }

    /// <summary>
    /// Create a Shutdown message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="scriptPubkey">The ScriptPubKey to send closing funds to.</param>
    /// <returns>The Shutdown message.</returns>
    /// <seealso cref="ShutdownMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="Script"/>
    /// <seealso cref="ShutdownPayload"/>
    public IMessage CreateShutdownMessage(ChannelId channelId, Script scriptPubkey)
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
    public IMessage CreateClosingSignedMessage(ChannelId channelId, ulong feeSatoshis, ECDSASignature signature,
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
    public IMessage CreateOpenChannel2Message(ChannelId temporaryChannelId, uint fundingFeeRatePerKw,
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
    public IMessage CreateAcceptChannel2Message(ChannelId temporaryChannelId, ulong fundingSatoshis,
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
    /// Create a UpdateAddHtlc message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="id">The htlc id.</param>
    /// <param name="amountMsat">The amount for this htlc.</param>
    /// <param name="paymentHash">The htlc payment hash.</param>
    /// <param name="cltvExpiry">The cltv expiry.</param>
    /// <param name="onionRoutingPacket">The onion routing packet.</param>
    /// <returns>The UpdateAddHtlc message.</returns>
    /// <seealso cref="UpdateAddHtlcMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="UpdateAddHtlcPayload"/>
    public IMessage CreateUpdateAddHtlcMessage(ChannelId channelId, ulong id, ulong amountMsat, ReadOnlyMemory<byte> paymentHash, uint cltvExpiry, ReadOnlyMemory<byte>? onionRoutingPacket = null)
    {
        var payload = new UpdateAddHtlcPayload(channelId, id, amountMsat, paymentHash, cltvExpiry, onionRoutingPacket);

        return new UpdateAddHtlcMessage(payload);
    }

    /// <summary>
    /// Create a UpdateFulfillHtlc message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="id">The htlc id.</param>
    /// <param name="preimage">The preimage for this htlc.</param>
    /// <returns>The UpdateFulfillHtlc message.</returns>
    /// <seealso cref="UpdateFulfillHtlcMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="UpdateFulfillHtlcPayload"/>
    public IMessage CreateUpdateFulfillHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> preimage)
    {
        var payload = new UpdateFulfillHtlcPayload(channelId, id, preimage);

        return new UpdateFulfillHtlcMessage(payload);
    }

    /// <summary>
    /// Create a UpdateFailHtlc message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="id">The htlc id.</param>
    /// <param name="reason">The reason for failure.</param>
    /// <returns>The UpdateFailHtlc message.</returns>
    /// <seealso cref="UpdateFailHtlcMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="UpdateFailHtlcPayload"/>
    public IMessage CreateUpdateFailHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> reason)
    {
        var payload = new UpdateFailHtlcPayload(channelId, id, reason);

        return new UpdateFailHtlcMessage(payload);
    }

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
    public IMessage CreateCommitmentSignedMessage(ChannelId channelId, ECDSASignature signature, IEnumerable<ECDSASignature> htlcSignatures)
    {
        var payload = new CommitmentSignedPayload(channelId, signature, htlcSignatures);

        return new CommitmentSignedMessage(payload);
    }

    /// <summary>
    /// Create a RevokeAndAck message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="perCommitmentSecret">The secret for the commitment transaction.</param>
    /// <param name="nextPerCommitmentPoint">The next per commitment point.</param>
    /// <returns>The RevokeAndAck message.</returns>
    /// <seealso cref="RevokeAndAckMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ECDSASignature"/>
    /// <seealso cref="RevokeAndAckPayload"/>
    public IMessage CreateCommitmentSignedMessage(ChannelId channelId, ReadOnlyMemory<byte> perCommitmentSecret, PubKey nextPerCommitmentPoint)
    {
        var payload = new RevokeAndAckPayload(channelId, perCommitmentSecret, nextPerCommitmentPoint);

        return new RevokeAndAckMessage(payload);
    }

    /// <summary>
    /// Create a UpdateFee message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="feeratePerKw">The fee rate for the commitment transaction.</param>
    /// <returns>The UpdateFee message.</returns>
    /// <seealso cref="UpdateFeeMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="UpdateFeePayload"/>
    public IMessage CreateUpdateFeeMessage(ChannelId channelId, uint feeratePerKw)
    {
        var payload = new UpdateFeePayload(channelId, feeratePerKw);

        return new UpdateFeeMessage(payload);
    }

    /// <summary>
    /// Create a UpdateFailMalformedHtlc message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="id">The htlc id.</param>
    /// <param name="sha256OfOnion">The sha256OfOnion for this htlc.</param>
    /// <param name="failureCode">The failureCode.</param>
    /// <returns>The UpdateFailMalformedHtlc message.</returns>
    /// <seealso cref="UpdateFailMalformedHtlcMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="UpdateFailMalformedHtlcPayload"/>
    public IMessage CreateUpdateFailMalformedHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> sha256OfOnion, ushort failureCode)
    {
        var payload = new UpdateFailMalformedHtlcPayload(channelId, id, sha256OfOnion, failureCode);

        return new UpdateFailMalformedHtlcMessage(payload);
    }

    /// <summary>
    /// Create a ChannelReestablish message.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="nextCommitmentNumber">The next commitment number.</param>
    /// <param name="nextRevocationNumber">The next revocation number.</param>
    /// <param name="yourLastPerCommitmentSecret">The peer last per commitment secret.</param>
    /// <param name="myCurrentPerCommitmentPoint">Our current per commitment point.</param>
    /// <returns>The ChannelReestablish message.</returns>
    /// <seealso cref="ChannelReestablishMessage"/>
    /// <seealso cref="ChannelId"/>
    /// <seealso cref="ChannelReestablishPayload"/>
    public IMessage CreateChannelReestablishMessage(ChannelId channelId, ulong nextCommitmentNumber,
                                                                ulong nextRevocationNumber,
                                                                ReadOnlyMemory<byte> yourLastPerCommitmentSecret,
                                                                PubKey myCurrentPerCommitmentPoint)
    {
        var payload = new ChannelReestablishPayload(channelId, nextCommitmentNumber, nextRevocationNumber,
                                                    yourLastPerCommitmentSecret, myCurrentPerCommitmentPoint);

        return new ChannelReestablishMessage(payload);
    }
    #endregion

    /// <summary>
    /// Deserialize a message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized message.</returns>
    /// <exception cref="InvalidMessageException">Unknown message type</exception>
    public async Task<IMessage?> DeserializeMessageAsync(MemoryStream stream)
    {
        // Get type of message
        var typeBytes = new byte[2];
        await stream.ReadExactlyAsync(typeBytes);
        var type = EndianBitConverter.ToUInt16BigEndian(typeBytes);

        // Deserialize message based on type
        switch (type)
        {
            case MessageTypes.WARNING:
                return await WarningMessage.DeserializeAsync(stream);                   // 01  -> 0x01
            case MessageTypes.INIT:
                return await InitMessage.DeserializeAsync(stream);                      // 16  -> 0x10
            case MessageTypes.ERROR:
                return await ErrorMessage.DeserializeAsync(stream);                     // 17  -> 0x11
            case MessageTypes.PING:
                return await PingMessage.DeserializeAsync(stream);                      // 18  -> 0x12
            case MessageTypes.PONG:
                return await PongMessage.DeserializeAsync(stream);                      // 19  -> 0x13
            case MessageTypes.CHANNEL_READY:
                return await ChannelReadyMessage.DeserializeAsync(stream);              // 36  -> 0x24
            case MessageTypes.SHUTDOWN:
                return await ShutdownMessage.DeserializeAsync(stream);                  // 38  -> 0x26
            case MessageTypes.CLOSING_SIGNED:
                return await ClosingSignedMessage.DeserializeAsync(stream);             // 39  -> 0x27
            case MessageTypes.OPEN_CHANNEL_2:
                return await OpenChannel2Message.DeserializeAsync(stream);              // 64  -> 0x40
            case MessageTypes.ACCEPT_CHANNEL_2:
                return await AcceptChannel2Message.DeserializeAsync(stream);            // 65  -> 0x41
            case MessageTypes.TX_ADD_INPUT:
                return await TxAddInputMessage.DeserializeAsync(stream);                // 66  -> 0x42
            case MessageTypes.TX_ADD_OUTPUT:
                return await TxAddOutputMessage.DeserializeAsync(stream);               // 67  -> 0x43
            case MessageTypes.TX_REMOVE_INPUT:
                return await TxRemoveInputMessage.DeserializeAsync(stream);             // 68  -> 0x44
            case MessageTypes.TX_REMOVE_OUTPUT:
                return await TxRemoveOutputMessage.DeserializeAsync(stream);            // 69  -> 0x45
            case MessageTypes.TX_COMPLETE:
                return await TxCompleteMessage.DeserializeAsync(stream);                // 70  -> 0x46
            case MessageTypes.TX_SIGNATURES:
                return await TxSignaturesMessage.DeserializeAsync(stream);              // 71  -> 0x47
            case MessageTypes.TX_INIT_RBF:
                return await TxInitRbfMessage.DeserializeAsync(stream);                 // 72  -> 0x48
            case MessageTypes.TX_ACK_RBF:
                return await TxAckRbfMessage.DeserializeAsync(stream);                  // 73  -> 0x49
            case MessageTypes.TX_ABORT:
                return await TxAbortMessage.DeserializeAsync(stream);                   // 74  -> 0x4A
            case MessageTypes.UPDATE_ADD_HTLC:
                return await UpdateAddHtlcMessage.DeserializeAsync(stream);             // 128 -> 0x80
            case MessageTypes.UPDATE_FULFILL_HTLC:
                return await UpdateFulfillHtlcMessage.DeserializeAsync(stream);         // 130 -> 0x82
            case MessageTypes.UPDATE_FAIL_HTLC:
                return await UpdateFailHtlcMessage.DeserializeAsync(stream);            // 131 -> 0x83
            case MessageTypes.COMMITMENT_SIGNED:
                return await CommitmentSignedMessage.DeserializeAsync(stream);          // 132 -> 0x84
            case MessageTypes.REVOKE_AND_ACK:
                return await RevokeAndAckMessage.DeserializeAsync(stream);              // 133 -> 0x85
            case MessageTypes.UPDATE_FEE:
                return await UpdateFeeMessage.DeserializeAsync(stream);                 // 134 -> 0x86
            case MessageTypes.UPDATE_FAIL_MALFORMED_HTLC:
                return await UpdateFailMalformedHtlcMessage.DeserializeAsync(stream);   // 135 -> 0x87
            case MessageTypes.CHANNEL_REESTABLISH:
                return await ChannelReestablishMessage.DeserializeAsync(stream);        // 136 -> 0x88

            case MessageTypes.OPEN_CHANNEL:
            case MessageTypes.ACCEPT_CHANNEL:
            case MessageTypes.FUNDING_CREATED:
            case MessageTypes.FUNDING_SIGNED:
                throw new InvalidMessageException("You must use OpenChannel2 flow");

            default:
                {
                    // If type is unknown and even, throw exception
                    if (type % 2 == 0)
                    {
                        throw new InvalidMessageException($"Unknown message type {type}");
                    }

                    return null;
                }
        }
    }
}