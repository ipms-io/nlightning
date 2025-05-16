using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Infrastructure.Converters;

namespace NLightning.Infrastructure.Serialization.Payloads.PayloadTypes;

using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Domain.ValueObjects;
using Exceptions;

public class TxInitRbfPayloadSerializer : IPayloadTypeSerializer<TxInitRbfPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TxInitRbfPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not TxInitRbfPayload txInitRbfPayload)
            throw new SerializationException($"Payload is not of type {nameof(TxInitRbfPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(txInitRbfPayload.ChannelId, stream);
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txInitRbfPayload.Locktime));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txInitRbfPayload.Feerate));
    }

    public async Task<TxInitRbfPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(uint));

        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var locktime = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            buffer = new byte[4];
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var feerate = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            return new TxInitRbfPayload(channelId, locktime, feerate);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(TxInitRbfPayload)}", e);
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