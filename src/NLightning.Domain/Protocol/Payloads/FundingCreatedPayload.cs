using NBitcoin.Crypto;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the funding_created message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the FundingCreatedPayload class.
/// </remarks>
public class FundingCreatedPayload : IChannelMessagePayload
{
    /// <summary>
    /// The temporary_channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// The funding transaction id.
    /// </summary>
    public ReadOnlyMemory<byte> FundingTxId { get; }

    /// <summary>
    /// The funding transaction output index.
    /// </summary>
    public ushort FundingOutputIndex { get; }

    /// <summary>
    /// The signature of the funding transaction.
    /// </summary>
    public ECDSASignature Signature { get; }

    public FundingCreatedPayload(ChannelId channelId, ReadOnlySpan<byte> fundingTxId, ushort fundingOutputIndex,
                                 ECDSASignature signature)
    {
        ChannelId = channelId;
        FundingTxId = fundingTxId.ToArray();
        FundingOutputIndex = fundingOutputIndex;
        Signature = signature;
    }
}