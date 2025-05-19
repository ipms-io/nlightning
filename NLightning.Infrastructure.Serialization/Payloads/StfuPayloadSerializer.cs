using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.ValueObjects;
using Exceptions;

public class StfuPayloadSerializer : IPayloadSerializer<StfuPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public StfuPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not StfuPayload stfuPayload)
            throw new SerializationException($"Payload is not of type {nameof(StfuPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(stfuPayload.ChannelId, stream);
        
        await stream.WriteAsync(new ReadOnlyMemory<byte>([(byte)(stfuPayload.Initiator ? 1 : 0)]));
    }

    public async Task<StfuPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(1);

        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..1]);

            return new StfuPayload(channelId, buffer[0] == 1);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(StfuPayload)}", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    async Task<IMessagePayload?> IPayloadSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}