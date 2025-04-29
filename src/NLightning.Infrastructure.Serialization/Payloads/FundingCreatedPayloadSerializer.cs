using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin.Crypto;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Crypto.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Domain.ValueObjects;
using Exceptions;

public class FundingCreatedPayloadSerializer : IPayloadSerializer<FundingCreatedPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public FundingCreatedPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not FundingCreatedPayload fundingCreatedPayload)
            throw new SerializationException($"Payload is not of type {nameof(FundingCreatedPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(fundingCreatedPayload.TemporaryChannelId, stream);

        await stream.WriteAsync(fundingCreatedPayload.FundingTxId);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(fundingCreatedPayload.FundingOutputIndex));
        await stream.WriteAsync(fundingCreatedPayload.Signature.ToCompact());
    }

    public async Task<FundingCreatedPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MAX_SIGNATURE_SIZE);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..HashConstants.SHA256_HASH_LEN]);
            var fundingTxId = buffer[..HashConstants.SHA256_HASH_LEN];

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var fundingOutputIndex = EndianBitConverter.ToUInt16BigEndian(buffer.AsSpan()[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MAX_SIGNATURE_SIZE]);
            if (!ECDSASignature.TryParseFromCompact(buffer[..CryptoConstants.MAX_SIGNATURE_SIZE], out var signature))
            {
                throw new Exception("Unable to parse signature");
            }

            return new FundingCreatedPayload(channelId, fundingTxId, fundingOutputIndex, signature);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(FundingCreatedPayload)}", e);
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