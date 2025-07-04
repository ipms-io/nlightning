using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Channels.ValueObjects;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Money;
using Domain.Protocol.Payloads;
using Domain.Serialization.Interfaces;
using Exceptions;

public class AcceptChannel1PayloadSerializer : IPayloadSerializer<AcceptChannel1Payload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public AcceptChannel1PayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not AcceptChannel1Payload acceptChannel1Payload)
            throw new SerializationException($"Payload is not of type {nameof(AcceptChannel1Payload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(acceptChannel1Payload.ChannelId, stream);

        // Serialize other types
        await stream
           .WriteAsync(EndianBitConverter.GetBytesBigEndian((ulong)acceptChannel1Payload.DustLimitAmount.Satoshi));
        await stream
           .WriteAsync(EndianBitConverter
                          .GetBytesBigEndian(acceptChannel1Payload.MaxHtlcValueInFlightAmount.MilliSatoshi));
        await stream
           .WriteAsync(EndianBitConverter
                          .GetBytesBigEndian((ulong)acceptChannel1Payload.ChannelReserveAmount.Satoshi));
        await stream
           .WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel1Payload.HtlcMinimumAmount.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel1Payload.MinimumDepth));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel1Payload.ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel1Payload.MaxAcceptedHtlcs));
        await stream.WriteAsync(acceptChannel1Payload.FundingPubKey);
        await stream.WriteAsync(acceptChannel1Payload.RevocationBasepoint);
        await stream.WriteAsync(acceptChannel1Payload.PaymentBasepoint);
        await stream.WriteAsync(acceptChannel1Payload.DelayedPaymentBasepoint);
        await stream.WriteAsync(acceptChannel1Payload.HtlcBasepoint);
        await stream.WriteAsync(acceptChannel1Payload.FirstPerCommitmentPoint);
    }

    public async Task<AcceptChannel1Payload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.CompactPubkeyLen);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var temporaryChannelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var dustLimitSatoshis = LightningMoney
               .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var maxHtlcValueInFlightMsat = EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var channelReserveAmount = LightningMoney
               .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var htlcMinimumMsat = EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var minimumDepth = EndianBitConverter.ToUInt32BigEndian(buffer.AsSpan()[..sizeof(uint)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(buffer.AsSpan()[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(buffer.AsSpan()[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var fundingPubKey = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var revocationBasepoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var paymentBasepoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var delayedPaymentBasepoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var htlcBasepoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var firstPerCommitmentPoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            return new AcceptChannel1Payload(temporaryChannelId, channelReserveAmount, delayedPaymentBasepoint,
                                             dustLimitSatoshis, firstPerCommitmentPoint, fundingPubKey, htlcBasepoint,
                                             htlcMinimumMsat, maxAcceptedHtlcs, maxHtlcValueInFlightMsat, minimumDepth,
                                             paymentBasepoint, revocationBasepoint, toSelfDelay);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing AcceptChannel1Payload", e);
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