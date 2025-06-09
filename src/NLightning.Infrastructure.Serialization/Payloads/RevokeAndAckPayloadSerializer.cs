using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Channels.ValueObjects;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.Payloads;
using Domain.Serialization.Interfaces;
using Exceptions;

public class RevokeAndAckPayloadSerializer : IPayloadSerializer<RevokeAndAckPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public RevokeAndAckPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not RevokeAndAckPayload revokeAndAckPayload)
            throw new SerializationException($"Payload is not of type {nameof(RevokeAndAckPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(revokeAndAckPayload.ChannelId, stream);

        await stream.WriteAsync(revokeAndAckPayload.PerCommitmentSecret);
        await stream.WriteAsync(revokeAndAckPayload.NextPerCommitmentPoint);
    }

    public async Task<RevokeAndAckPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.CompactPubkeyLen);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            var perCommitmentSecret = new byte[32];
            await stream.ReadExactlyAsync(perCommitmentSecret);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var nextPerCommitmentPoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            return new RevokeAndAckPayload(channelId, nextPerCommitmentPoint, perCommitmentSecret);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(RevokeAndAckPayload)}", e);
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