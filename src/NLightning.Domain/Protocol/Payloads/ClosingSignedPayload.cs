using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Money;
using ValueObjects;

/// <summary>
/// Represents the payload for the closing_signed message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ClosingSignedPayload class.
/// </remarks>
public class ClosingSignedPayload : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id is used to identify this channel.
    /// </summary>
    public ChannelId ChannelId { get; set; }

    /// <summary>
    /// funding_satoshis is the amount the acceptor is putting into the channel.
    /// </summary>
    public LightningMoney FeeAmount { get; set; }

    /// <summary>
    /// The signature for the closing transaction
    /// </summary>
    public CompactSignature Signature { get; }

    public ClosingSignedPayload(ChannelId channelId, LightningMoney feeAmount, CompactSignature signature)
    {
        ChannelId = channelId;
        FeeAmount = feeAmount;
        Signature = signature;
    }
}