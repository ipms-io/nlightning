namespace NLightning.Bolts.BOLT1.Payloads;

using System.IO;
using System.Threading.Tasks;
using Bolts.Interfaces;
using NLightning.Common;

public class ErrorPayload : IMessagePayload
{
    public ChannelId ChannelId { get; } = ChannelId.Zero;
    public byte[] Data { get; }

    public ErrorPayload(byte[] data)
    {
        Data = data;
    }
    public ErrorPayload(ChannelId channelId, byte[] data) : this(data)
    {
        ChannelId = channelId;
    }
    public ErrorPayload(BinaryReader reader)
    {
        ChannelId = new ChannelId(reader);
        Data = reader.ReadBytes(EndianBitConverter.ToUInt16BE(reader.ReadBytes(2)));
    }

    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBE((ushort)Data.Length));
        await stream.WriteAsync(Data);
    }
}