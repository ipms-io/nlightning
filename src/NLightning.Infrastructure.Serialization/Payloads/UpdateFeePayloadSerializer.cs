using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Exceptions;

public class UpdateFeePayloadSerializer : IPayloadSerializer<UpdateFeePayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public UpdateFeePayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not UpdateFeePayload updateFeePayload)
            throw new SerializationException($"Payload is not of type {nameof(UpdateFeePayload)}");

        // Get the ChannelId serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(updateFeePayload.ChannelId, stream);

        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(updateFeePayload.FeeratePerKw));
    }

    public async Task<UpdateFeePayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(uint));

        try
        {
            // Get the ChannelId serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var feeratePerKw = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            return new UpdateFeePayload(channelId, feeratePerKw);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(UpdateFeePayload)}", e);
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