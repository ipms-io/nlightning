using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Constants;
using Interfaces;

public class TxAddInputPayload : IMessagePayload
{
    public ChannelId ChannelId { get; }
    public ulong SerialId { get; }
    public byte[] PrevTx { get; }
    public uint PrevTxVout { get; }
    public uint Sequence { get; }

    public TxAddInputPayload(ChannelId channelId, ulong serialId, byte[] prevTx, uint prevTxVout, uint sequence)
    {
        if (sequence > InputContants.MAX_SEQUENCE)
        {
            throw new ArgumentException($"Sequence must be less than or equal to {InputContants.MAX_SEQUENCE}", nameof(sequence));
        }

        ChannelId = channelId;
        SerialId = serialId;
        PrevTx = prevTx;
        PrevTxVout = prevTxVout;
        Sequence = sequence;
    }

    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(SerialId));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)PrevTx.Length));
        await stream.WriteAsync(PrevTx);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(PrevTxVout));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Sequence));
    }

    public static async Task<TxAddInputPayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[8];
            await stream.ReadExactlyAsync(bytes);
            var serialId = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[2];
            await stream.ReadExactlyAsync(bytes);
            var prevTxLength = EndianBitConverter.ToUInt16BigEndian(bytes);

            var prevTx = new byte[prevTxLength];
            await stream.ReadExactlyAsync(prevTx);

            bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            var prevTxVout = EndianBitConverter.ToUInt32BigEndian(bytes);

            bytes = new byte[4];
            await stream.ReadExactlyAsync(bytes);
            var sequence = EndianBitConverter.ToUInt32BigEndian(bytes);

            return new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout, sequence);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxAddInputPayload", e);
        }
    }
}