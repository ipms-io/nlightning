using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.Types;
using Common.Utils;
using Interfaces;

public class TxAddOutputPayload : IMessagePayload
{
    public ChannelId ChannelId { get; }
    public ulong SerialId { get; }
    public ulong Sats { get; }
    public byte[] Script { get; }

    public TxAddOutputPayload(ChannelId channelId, ulong serialId, ulong sats, byte[] script)
    {
        if (script.Length > ushort.MaxValue)
        {
            throw new ArgumentException($"Script length must be less than or equal to {ushort.MaxValue} bytes", nameof(script));
        }

        ChannelId = channelId;
        SerialId = serialId;
        Sats = sats;
        Script = script;
    }

    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBE(SerialId));
        await stream.WriteAsync(EndianBitConverter.GetBytesBE(Sats));
        await stream.WriteAsync(EndianBitConverter.GetBytesBE((ushort)Script.Length));
        await stream.WriteAsync(Script);
    }

    public static async Task<TxAddOutputPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BE(bytes);

            bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var sats = EndianBitConverter.ToUInt64BE(bytes);

            bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var scriptLength = EndianBitConverter.ToUInt16BE(bytes);

            var script = new byte[scriptLength];
            await stream.ReadExactlyAsync(script);

            return new TxAddOutputPayload(channelId, serialId, sats, script);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxAddOutputPayload", e);
        }
    }
}