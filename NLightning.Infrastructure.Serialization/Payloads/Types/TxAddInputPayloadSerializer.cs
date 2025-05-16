using System.Buffers;
using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Payloads.Types;

using Converters;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads.Types;
using Domain.ValueObjects;
using Exceptions;

public class TxAddInputPayloadSerializer : IPayloadTypeSerializer<TxAddInputPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TxAddInputPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not TxAddInputPayload txAddInputPayload)
            throw new SerializationException($"Payload is not of type {nameof(TxAddInputPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(txAddInputPayload.ChannelId, stream);
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txAddInputPayload.SerialId));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)txAddInputPayload.PrevTx.Length));
        await stream.WriteAsync(txAddInputPayload.PrevTx);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txAddInputPayload.PrevTxVout));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txAddInputPayload.Sequence));
    }

    public async Task<TxAddInputPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));

        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var serialId = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var prevTxLength = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            var prevTx = new byte[prevTxLength];
            await stream.ReadExactlyAsync(prevTx);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var prevTxVout = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var sequence = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            return new TxAddInputPayload(channelId, serialId, prevTx, prevTxVout, sequence);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(TxAddInputPayload)}", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    async Task<IMessagePayload?> IPayloadTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}