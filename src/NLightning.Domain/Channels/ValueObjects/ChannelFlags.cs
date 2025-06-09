namespace NLightning.Domain.Channels.ValueObjects;

using Domain.Enums;
using Domain.Interfaces;

/// <summary>
/// Only the least-significant bit of channel_flags is currently defined: announce_channel. This indicates whether
/// the initiator of the funding flow wishes to advertise this channel publicly to the network
/// </summary>
public readonly record struct ChannelFlags : IValueObject, IEquatable<ChannelFlags>
{
    private readonly byte _value;

    public bool AnnounceChannel => ((ChannelFlag)_value).HasFlag(ChannelFlag.AnnounceChannel);

    public ChannelFlags(byte value)
    {
        _value = value;
    }

    public ChannelFlags(ChannelFlag value)
    {
        _value = (byte)value;
    }

    #region Implicit Conversions

    public static implicit operator byte(ChannelFlags c) => c._value;
    public static implicit operator ChannelFlags(byte value) => new(value);
    public static implicit operator byte[](ChannelFlags c) => [c._value];
    public static implicit operator ChannelFlags(byte[] value) => new(value[0]);

    #endregion
}