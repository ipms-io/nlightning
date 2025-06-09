using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Channels.ValueObjects;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.Payloads;
using Exceptions;

public class ChannelReadyPayloadSerializer : IPayloadSerializer<ChannelReadyPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ChannelReadyPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ChannelReadyPayload channelReadyPayload)
            throw new SerializationException($"Payload is not of type {nameof(ChannelReadyPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(channelReadyPayload.ChannelId, stream);

        // Serialize other types
        await stream.WriteAsync(channelReadyPayload.SecondPerCommitmentPoint);
    }

    public async Task<ChannelReadyPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.CompactPubkeyLen);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var secondPerCommitmentPoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            return new ChannelReadyPayload(channelId, secondPerCommitmentPoint);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ChannelReadyPayload", e);
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