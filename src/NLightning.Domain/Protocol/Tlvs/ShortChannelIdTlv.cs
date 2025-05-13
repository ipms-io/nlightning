using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Models;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

/// <summary>
/// Short Channel Id TLV.
/// </summary>
/// <remarks>
/// The short channel id TLV is used in the ChannelReadyMessage to communicate the channel type that should be opened.
/// </remarks>
public class ShortChannelIdTlv : BaseTlv
{
    /// <summary>
    /// The shutdown script to be used when closing the channel
    /// </summary>
    public ShortChannelId ShortChannelId { get; }

    public ShortChannelIdTlv(ShortChannelId shortChannelId) : base(TlvConstants.ShortChannelId)
    {
        ShortChannelId = shortChannelId;

        Value = shortChannelId;
        Length = Value.Length;
    }

    /// <summary>
    /// Cast ShortChannelIdTlv from a BaseTlv.
    /// </summary>
    /// <param name="baseTlv">The baseTlv to cast from.</param>
    /// <returns>The cast ShortChannelIdTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting ShortChannelIdTlv</exception>
    public static ShortChannelIdTlv FromTlv(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.ShortChannelId)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ShortChannelIdTlv(baseTlv.Value);
    }
}