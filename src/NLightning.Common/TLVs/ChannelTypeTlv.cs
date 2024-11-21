namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Channel Type TLV.
/// </summary>
/// <remarks>
/// The channels type TLV is used in the AcceptChannel2Message to communicate the channel type that should be opened.
/// </remarks>
public class ChannelTypeTlv : Tlv
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

    /// <summary>
    /// Cast ChannelTypeTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast ChannelTypeTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting ChannelTypeTlv</exception>
    public static ChannelTypeTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.CHANNEL_TYPE)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ChannelTypeTlv(tlv.Value);
    }
}