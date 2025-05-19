namespace NLightning.Domain.Protocol.Tlv;

using Constants;

/// <summary>
/// Channel Type TLV.
/// </summary>
/// <remarks>
/// The channels type TLV is used in the AcceptChannel2Message to communicate the channel type that should be opened.
/// </remarks>
public class ChannelTypeTlv : BaseTlv
{
    /// <summary>
    /// The channel type
    /// </summary>
    public byte[] ChannelType { get; }

    public ChannelTypeTlv(byte[] channelType) : base(TlvConstants.CHANNEL_TYPE)
    {
        ChannelType = channelType;

        Value = channelType;
        Length = Value.Length;
    }
}