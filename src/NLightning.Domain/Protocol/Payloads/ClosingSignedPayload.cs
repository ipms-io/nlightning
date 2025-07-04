namespace NLightning.Domain.Protocol.Payloads;

using Channels.ValueObjects;
using Crypto.ValueObjects;
using Interfaces;
using Money;

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