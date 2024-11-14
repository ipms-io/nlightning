namespace NLightning.Common.Types;

/// <summary>
/// Only the least-significant bit of channel_flags is currently defined: announce_channel. This indicates whether
/// the initiator of the funding flow wishes to advertise this channel publicly to the network
/// </summary>
public readonly struct ChannelFlags
{
    private readonly byte _value;
    public readonly bool ANNOUNCE_CHANNEL;

    public ChannelFlags(byte value)
    {
        _value = value;
        ANNOUNCE_CHANNEL = (value & 1) == 1;
    }

    public ChannelFlags(bool announceChannel)
    {
        ANNOUNCE_CHANNEL = announceChannel;
        _value = announceChannel ? (byte)(_value | 1) : (byte)(_value & ~1);
    }

    public ValueTask SerializeAsync(Stream stream)
    {
        return stream.WriteAsync(new ReadOnlyMemory<byte>([_value]));
    }

    public static async Task<ChannelFlags> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[1];
        await stream.ReadExactlyAsync(buffer);
        return new ChannelFlags(buffer[0]);
    }

    #region Operators
    public static implicit operator byte(ChannelFlags c) => c._value;
    public static implicit operator ChannelFlags(byte value) => new(value);
    public static implicit operator byte[](ChannelFlags c) => [c._value];
    public static implicit operator ChannelFlags(byte[] value) => new(value[0]);
    #endregion
}