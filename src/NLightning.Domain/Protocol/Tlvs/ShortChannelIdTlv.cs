using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

/// <summary>
/// Short Channel Id TLV.
/// </summary>
/// <remarks>
/// The short channel id TLV is used in the ChannelReadyMessage to communicate the channel type that should be opened.
/// </remarks>
public class ShortChannelIdTlv : Tlv
{
    /// <summary>
    /// The shutdown script to be used when closing the channel
    /// </summary>
    public ShortChannelId ShortChannelId { get; }

    public ShortChannelIdTlv(ShortChannelId shortChannelId) : base(TlvConstants.SHORT_CHANNEL_ID)
    {
        ShortChannelId = shortChannelId;

        Value = shortChannelId;
        Length = Value.Length;
    }

    /// <summary>
    /// Cast ShortChannelIdTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast ShortChannelIdTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting ShortChannelIdTlv</exception>
    public static ShortChannelIdTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.SHORT_CHANNEL_ID)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ShortChannelIdTlv(tlv.Value);
    }
}