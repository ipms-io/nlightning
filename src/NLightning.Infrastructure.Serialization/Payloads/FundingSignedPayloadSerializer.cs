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

public class FundingSignedPayloadSerializer : IPayloadSerializer<FundingSignedPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public FundingSignedPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not FundingSignedPayload fundingSignedPayload)
            throw new SerializationException($"Payload is not of type {nameof(FundingSignedPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(fundingSignedPayload.ChannelId, stream);

        await stream.WriteAsync(fundingSignedPayload.Signature);
    }

    public async Task<FundingSignedPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MaxSignatureSize);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MaxSignatureSize]);
            var signature = new CompactSignature(buffer[..CryptoConstants.MaxSignatureSize]);

            return new FundingSignedPayload(channelId, signature);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(FundingSignedPayload)}", e);
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