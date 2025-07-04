using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Exceptions;

public class TxRemoveOutputPayloadSerializer : IPayloadSerializer<TxRemoveOutputPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TxRemoveOutputPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not TxRemoveOutputPayload txRemoveOutputPayload)
            throw new SerializationException($"Payload is not of type {nameof(TxRemoveOutputPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(txRemoveOutputPayload.ChannelId, stream);

        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txRemoveOutputPayload.SerialId));
    }

    public async Task<TxRemoveOutputPayload?> DeserializeAsync(Stream stream)
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

            return new TxRemoveOutputPayload(channelId, serialId);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(TxRemoveOutputPayload)}", e);
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