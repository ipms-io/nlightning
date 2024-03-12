namespace NLightning.Bolts.BOLT1.Types;

public class ErrorData(ChannelId channelId, byte[] data)
{
    public ChannelId ChannelId { get; } = channelId;
    public U16 Len => (byte)data.Length;
    public byte[] Data { get; } = data;
}