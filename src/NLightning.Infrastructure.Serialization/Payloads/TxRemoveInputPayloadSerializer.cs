using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Exceptions;

public class TxRemoveInputPayloadSerializer : IPayloadSerializer<TxRemoveInputPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TxRemoveInputPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not TxRemoveInputPayload txRemoveInputPayload)
            throw new SerializationException($"Payload is not of type {nameof(TxRemoveInputPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(txRemoveInputPayload.ChannelId, stream);

        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(txRemoveInputPayload.SerialId));
    }

    public async Task<TxRemoveInputPayload?> DeserializeAsync(Stream stream)
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

            return new TxRemoveInputPayload(channelId, serialId);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(TxRemoveInputPayload)}", e);
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