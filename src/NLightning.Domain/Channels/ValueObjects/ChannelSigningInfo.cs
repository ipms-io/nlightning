namespace NLightning.Domain.Channels.ValueObjects;

using Bitcoin.ValueObjects;
using Crypto.ValueObjects;

/// <summary>
/// Information needed by the signer for a specific channel
/// </summary>
public record struct ChannelSigningInfo
{
    public TxId FundingTxId { get; init; }
    public uint FundingOutputIndex { get; init; }
    public ulong FundingSatoshis { get; init; }
    public CompactPubKey LocalFundingPubKey { get; init; }
    public CompactPubKey RemoteFundingPubKey { get; init; }
    public uint ChannelKeyIndex { get; init; } // For deterministic key derivation

    public ChannelSigningInfo(TxId fundingTxId, uint fundingOutputIndex, ulong fundingSatoshis,
                              CompactPubKey localFundingPubKey, CompactPubKey remoteFundingPubKey,
                              uint channelKeyIndex)
    {
        FundingTxId = fundingTxId;
        FundingOutputIndex = fundingOutputIndex;
        FundingSatoshis = fundingSatoshis;
        LocalFundingPubKey = localFundingPubKey;
        RemoteFundingPubKey = remoteFundingPubKey;
        ChannelKeyIndex = channelKeyIndex;
    }
}