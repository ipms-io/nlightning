using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Channels.ValueObjects;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Payloads;
using Exceptions;

public class ClosingSignedPayloadSerializer : IPayloadSerializer<ClosingSignedPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ClosingSignedPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ClosingSignedPayload closingSignedPayload)
            throw new SerializationException($"Payload is not of type {nameof(ClosingSignedPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(closingSignedPayload.ChannelId, stream);

        // Serialize other types
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(closingSignedPayload.FeeAmount.Satoshi));
        await stream.WriteAsync(closingSignedPayload.Signature);
    }

    public async Task<ClosingSignedPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MaxSignatureSize);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var feeSatoshis = LightningMoney.FromUnit(EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]),
                                                      LightningMoneyUnit.Satoshi);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.MaxSignatureSize]);
            var signature = new CompactSignature(buffer[..CryptoConstants.MaxSignatureSize]);

            return new ClosingSignedPayload(channelId, feeSatoshis, signature);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ClosingSignedPayload", e);
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