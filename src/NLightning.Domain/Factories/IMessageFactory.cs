using NBitcoin;
using NBitcoin.Crypto;
using NLightning.Domain.Protocol.Messages;

namespace NLightning.Domain.Factories;

using Money;
using Protocol.Messages.Interfaces;
using ValueObjects;

public interface IMessageFactory
{
    InitMessage CreateInitMessage();
    WarningMessage CreateWarningMessage(string message, ChannelId? channelId);
    WarningMessage CreateWarningMessage(byte[] data, ChannelId? channelId);
    StfuMessage CreateStfuMessage(ChannelId channelId, bool initiator);
    ErrorMessage CreateErrorMessage(string message, ChannelId? channelId);
    ErrorMessage CreateErrorMessage(byte[] data, ChannelId? channelId);
    PingMessage CreatePingMessage();
    PongMessage CreatePongMessage(IMessage pingMessage);
    TxAddInputMessage CreateTxAddInputMessage(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout,
                                     uint sequence);

    TxAddOutputMessage CreateTxAddOutputMessage(ChannelId channelId, ulong serialId, LightningMoney amount, Script script);
    TxRemoveInputMessage CreateTxRemoveInputMessage(ChannelId channelId, ulong serialId);
    TxRemoveOutputMessage CreateTxRemoveOutputMessage(ChannelId channelId, ulong serialId);
    TxCompleteMessage CreateTxCompleteMessage(ChannelId channelId);
    TxSignaturesMessage CreateTxSignaturesMessage(ChannelId channelId, byte[] txId, List<Witness> witnesses);

    TxInitRbfMessage CreateTxInitRbfMessage(ChannelId channelId, uint locktime, uint feerate, long fundingOutputContrubution,
                                    bool requireConfirmedInputs);
    TxAckRbfMessage CreateTxAckRbfMessage(ChannelId channelId, long fundingOutputContrubution, bool requireConfirmedInputs);
    TxAbortMessage CreateTxAbortMessage(ChannelId channelId, byte[] data);
    ChannelReadyMessage CreateChannelReadyMessage(ChannelId channelId, PubKey secondPerCommitmentPoint,
                                       ShortChannelId? shortChannelId = null);
    ShutdownMessage CreateShutdownMessage(ChannelId channelId, Script scriptPubkey);
    ClosingSignedMessage CreateClosingSignedMessage(ChannelId channelId, ulong feeSatoshis, ECDSASignature signature,
                                        ulong minFeeSatoshis, ulong maxFeeSatoshis);
    OpenChannel2Message CreateOpenChannel2Message(ChannelId temporaryChannelId, uint fundingFeeRatePerKw,
                                       uint commitmentFeeRatePerKw, ulong fundingSatoshis, PubKey fundingPubKey,
                                       PubKey revocationBasepoint, PubKey paymentBasepoint,
                                       PubKey delayedPaymentBasepoint, PubKey htlcBasepoint,
                                       PubKey firstPerCommitmentPoint, PubKey secondPerCommitmentPoint,
                                       ChannelFlags channelFlags, Script? shutdownScriptPubkey = null,
                                       byte[]? channelType = null, bool requireConfirmedInputs = false);
    AcceptChannel2Message CreateAcceptChannel2Message(ChannelId temporaryChannelId, LightningMoney fundingSatoshis,
                                         PubKey fundingPubKey, PubKey revocationBasepoint, PubKey paymentBasepoint,
                                         PubKey delayedPaymentBasepoint, PubKey htlcBasepoint,
                                         PubKey firstPerCommitmentPoint, Script? shutdownScriptPubkey = null,
                                         byte[]? channelType = null, bool requireConfirmedInputs = false);
    UpdateAddHtlcMessage CreateUpdateAddHtlcMessage(ChannelId channelId, ulong id, ulong amountMsat,
                                        ReadOnlyMemory<byte> paymentHash, uint cltvExpiry,
                                        ReadOnlyMemory<byte>? onionRoutingPacket = null);
    UpdateFulfillHtlcMessage CreateUpdateFulfillHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> preimage);
    UpdateFailHtlcMessage CreateUpdateFailHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> reason);
    CommitmentSignedMessage CreateCommitmentSignedMessage(ChannelId channelId, ECDSASignature signature,
                                           IEnumerable<ECDSASignature> htlcSignatures);
    RevokeAndAckMessage CreateRevokeAndAckMessage(ChannelId channelId, ReadOnlyMemory<byte> perCommitmentSecret,
                                           PubKey nextPerCommitmentPoint);
    UpdateFeeMessage CreateUpdateFeeMessage(ChannelId channelId, uint feeratePerKw);
    UpdateFailMalformedHtlcMessage CreateUpdateFailMalformedHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> sha256OfOnion,
                                                  ushort failureCode);
    ChannelReestablishMessage CreateChannelReestablishMessage(ChannelId channelId, ulong nextCommitmentNumber,
                                             ulong nextRevocationNumber,
                                             ReadOnlyMemory<byte> yourLastPerCommitmentSecret,
                                             PubKey myCurrentPerCommitmentPoint);
}