namespace NLightning.Domain.Protocol.Tlv;

using Channels.ValueObjects;
using Constants;

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
}