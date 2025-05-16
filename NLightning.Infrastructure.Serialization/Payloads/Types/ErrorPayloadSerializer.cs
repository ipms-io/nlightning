using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Payloads.Types;

using Converters;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads.Types;
using Domain.ValueObjects;
using Exceptions;

public class ErrorPayloadSerializer : IPayloadTypeSerializer<ErrorPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ErrorPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ErrorPayload errorPayload)
            throw new SerializationException($"Payload is not of type {nameof(ErrorPayload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(errorPayload.ChannelId, stream);
        
        // Serialize other types
        if (errorPayload.Data is null)
        {
            await stream.WriteAsync("\0\0"u8.ToArray());
        }
        else
        {
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)errorPayload.Data.Length));
            await stream.WriteAsync(errorPayload.Data);
        }
    }

    public async Task<ErrorPayload?> DeserializeAsync(Stream stream)
    {
        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            var buffer = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(buffer);
            var dataLength = EndianBitConverter.ToUInt16BigEndian(buffer);

            buffer = new byte[dataLength];
            await stream.ReadExactlyAsync(buffer);

            return new ErrorPayload(channelId, buffer);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ErrorPayload", e);
        }
    }

    async Task<IMessagePayload?> IPayloadTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}