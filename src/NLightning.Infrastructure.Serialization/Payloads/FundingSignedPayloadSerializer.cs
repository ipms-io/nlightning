using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin.Crypto;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Crypto.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Domain.ValueObjects;
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

        await stream.WriteAsync(fundingSignedPayload.Signature.ToCompact());
    }

    public async Task<FundingSignedPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MAX_SIGNATURE_SIZE);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MAX_SIGNATURE_SIZE]);
            if (!ECDSASignature.TryParseFromCompact(buffer[..CryptoConstants.MAX_SIGNATURE_SIZE], out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

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