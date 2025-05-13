using NLightning.Domain.Protocol.Models;

namespace NLightning.Domain.Protocol.Tlvs;

using Constants;
using ValueObjects;

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

    public ChannelTypeTlv(byte[] channelType) : base(TlvConstants.ChannelType)
    {
        ChannelType = channelType;

        Value = channelType;
        Length = Value.Length;
    }

    /// <summary>
    /// Cast ChannelTypeTlv from a BaseTlv.
    /// </summary>
    /// <param name="baseTlv">The baseTlv to cast from.</param>
    /// <returns>The cast ChannelTypeTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting ChannelTypeTlv</exception>
    public static ChannelTypeTlv FromTlv(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.ChannelType)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ChannelTypeTlv(baseTlv.Value);
    }
}