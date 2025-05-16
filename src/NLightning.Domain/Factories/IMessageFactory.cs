using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Domain.Factories;

using Money;
using Protocol.Messages.Interfaces;
using ValueObjects;

public interface IMessageFactory
{
    IMessage CreateInitMessage();
    IMessage CreateWarningMessage(string message, ChannelId? channelId);
    IMessage CreateWarningMessage(byte[] data, ChannelId? channelId);
    IMessage CreateStfuMessage(ChannelId channelId, bool initiator);
    IMessage CreateErrorMessage(string message, ChannelId? channelId);
    IMessage CreateErrorMessage(byte[] data, ChannelId? channelId);
    IMessage CreatePingMessage();
    IMessage CreatePongMessage(IMessage pingMessage);
    IMessage CreateTxAddInputMessage(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout,
                                     uint sequence);

    IMessage CreateTxAddOutputMessage(ChannelId channelId, ulong serialId, LightningMoney amount, Script script);
    IMessage CreateTxRemoveInputMessage(ChannelId channelId, ulong serialId);
    IMessage CreateTxRemoveOutputMessage(ChannelId channelId, ulong serialId);
    IMessage CreateTxCompleteMessage(ChannelId channelId);
    IMessage CreateTxSignaturesMessage(ChannelId channelId, byte[] txId, List<Witness> witnesses);

    IMessage CreateTxInitRbfMessage(ChannelId channelId, uint locktime, uint feerate, long fundingOutputContrubution,
                                    bool requireConfirmedInputs);
    IMessage CreateTxAckRbfMessage(ChannelId channelId, long fundingOutputContrubution, bool requireConfirmedInputs);
    IMessage CreateTxAbortMessage(ChannelId channelId, byte[] data);
    IMessage CreateChannelReadyMessage(ChannelId channelId, PubKey secondPerCommitmentPoint,
                                       ShortChannelId? shortChannelId = null);
    IMessage CreateShutdownMessage(ChannelId channelId, Script scriptPubkey);
    IMessage CreateClosingSignedMessage(ChannelId channelId, ulong feeSatoshis, ECDSASignature signature,
                                        ulong minFeeSatoshis, ulong maxFeeSatoshis);
    IMessage CreateOpenChannel2Message(ChannelId temporaryChannelId, uint fundingFeeRatePerKw,
                                       uint commitmentFeeRatePerKw, ulong fundingSatoshis, PubKey fundingPubKey,
                                       PubKey revocationBasepoint, PubKey paymentBasepoint,
                                       PubKey delayedPaymentBasepoint, PubKey htlcBasepoint,
                                       PubKey firstPerCommitmentPoint, PubKey secondPerCommitmentPoint,
                                       ChannelFlags channelFlags, Script? shutdownScriptPubkey = null,
                                       byte[]? channelType = null, bool requireConfirmedInputs = false);
    IMessage CreateAcceptChannel2Message(ChannelId temporaryChannelId, LightningMoney fundingSatoshis,
                                         PubKey fundingPubKey, PubKey revocationBasepoint, PubKey paymentBasepoint,
                                         PubKey delayedPaymentBasepoint, PubKey htlcBasepoint,
                                         PubKey firstPerCommitmentPoint, Script? shutdownScriptPubkey = null,
                                         byte[]? channelType = null, bool requireConfirmedInputs = false);
    IMessage CreateUpdateAddHtlcMessage(ChannelId channelId, ulong id, ulong amountMsat,
                                        ReadOnlyMemory<byte> paymentHash, uint cltvExpiry,
                                        ReadOnlyMemory<byte>? onionRoutingPacket = null);
    IMessage CreateUpdateFulfillHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> preimage);
    IMessage CreateUpdateFailHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> reason);
    IMessage CreateCommitmentSignedMessage(ChannelId channelId, ECDSASignature signature,
                                           IEnumerable<ECDSASignature> htlcSignatures);
    IMessage CreateCommitmentSignedMessage(ChannelId channelId, ReadOnlyMemory<byte> perCommitmentSecret,
                                           PubKey nextPerCommitmentPoint);
    IMessage CreateUpdateFeeMessage(ChannelId channelId, uint feeratePerKw);
    IMessage CreateUpdateFailMalformedHtlcMessage(ChannelId channelId, ulong id, ReadOnlyMemory<byte> sha256OfOnion,
                                                  ushort failureCode);
    IMessage CreateChannelReestablishMessage(ChannelId channelId, ulong nextCommitmentNumber,
                                             ulong nextRevocationNumber,
                                             ReadOnlyMemory<byte> yourLastPerCommitmentSecret,
                                             PubKey myCurrentPerCommitmentPoint);
}