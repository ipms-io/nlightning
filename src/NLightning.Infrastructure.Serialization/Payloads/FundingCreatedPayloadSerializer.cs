using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Bitcoin.ValueObjects;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Converters;
using Domain.Crypto.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
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
        await channelIdSerializer.SerializeAsync(fundingCreatedPayload.ChannelId, stream);

        await stream.WriteAsync(fundingCreatedPayload.FundingTxId);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(fundingCreatedPayload.FundingOutputIndex));
        await stream.WriteAsync(fundingCreatedPayload.Signature);
    }

    public async Task<FundingCreatedPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MaxSignatureSize);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.Sha256HashLen]);
            var fundingTxId = new TxId(buffer[..CryptoConstants.Sha256HashLen]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var fundingOutputIndex = EndianBitConverter.ToUInt16BigEndian(buffer.AsSpan()[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MaxSignatureSize]);
            var signature = new CompactSignature(buffer[..CryptoConstants.MaxSignatureSize]);

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