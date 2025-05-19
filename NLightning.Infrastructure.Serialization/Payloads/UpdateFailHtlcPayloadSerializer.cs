using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.ValueObjects;
using Exceptions;

public class UpdateFailHtlcPayloadSerializer : IPayloadSerializer<UpdateFailHtlcPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public UpdateFailHtlcPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not UpdateFailHtlcPayload updateFailHtlcPayload)
            throw new SerializationException($"Payload is not of type {nameof(UpdateFailHtlcPayload)}");
        
        // Get the ChannelId serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(updateFailHtlcPayload.ChannelId, stream);
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(updateFailHtlcPayload.Id));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(updateFailHtlcPayload.Len));
        await stream.WriteAsync(updateFailHtlcPayload.Reason);
    }

    public async Task<UpdateFailHtlcPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));

        try
        {
            // Get the ChannelId serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var id = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var len = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            var reason = new byte[len];
            await stream.ReadExactlyAsync(reason);

            return new UpdateFailHtlcPayload(channelId, id, reason);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(UpdateFailHtlcPayload)}", e);
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