using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Crypto.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.ValueObjects;
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
        await stream.WriteAsync(revokeAndAckPayload.NextPerCommitmentPoint.ToBytes());
    }

    public async Task<RevokeAndAckPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.PUBKEY_LEN);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            var perCommitmentSecret = new byte[32];
            await stream.ReadExactlyAsync(perCommitmentSecret);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var nextPerCommitmentPoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

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