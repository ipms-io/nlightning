using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the funding_created message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the FundingCreatedPayload class.
/// </remarks>
public class FundingSignedPayload : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// The signature of the funding transaction.
    /// </summary>
    public CompactSignature Signature { get; }

    public FundingSignedPayload(ChannelId channelId, CompactSignature signature)
    {
        ChannelId = channelId;
        Signature = signature;
    }
}