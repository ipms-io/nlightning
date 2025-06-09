namespace NLightning.Domain.Models;

using Channels.ValueObjects;
using Crypto.ValueObjects;

/// <summary>
/// Represents routing information for a payment
/// </summary>
/// <param name="compactPubKey">The public key of the node</param>
/// <param name="shortChannelId">The short channel id of the channel</param>
/// <param name="feeBaseMsat">The base fee in millisatoshis</param>
/// <param name="feeProportionalMillionths">The proportional fee in millionths</param>
/// <param name="cltvExpiryDelta">The CLTV expiry delta</param>
public sealed class RoutingInfo(
    CompactPubKey compactPubKey,
    ShortChannelId shortChannelId,
    int feeBaseMsat,
    int feeProportionalMillionths,
    short cltvExpiryDelta)
{
    /// <summary>
    /// The public key of the node
    /// </summary>
    public CompactPubKey CompactPubKey { get; } = compactPubKey;

    /// <summary>
    /// The short channel id of the channel
    /// </summary>
    public ShortChannelId ShortChannelId { get; } = shortChannelId;

    /// <summary>
    /// The base fee in millisatoshis
    /// </summary>
    public int FeeBaseMsat { get; } = feeBaseMsat;

    /// <summary>
    /// The proportional fee in millionths
    /// </summary>
    public int FeeProportionalMillionths { get; } = feeProportionalMillionths;

    /// <summary>
    /// The CLTV expiry delta
    /// </summary>
    public short CltvExpiryDelta { get; } = cltvExpiryDelta;
}