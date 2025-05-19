using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Crypto.Constants;
using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.ValueObjects;
using Exceptions;

public class AcceptChannel2PayloadSerializer : IPayloadSerializer<AcceptChannel2Payload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public AcceptChannel2PayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not AcceptChannel2Payload acceptChannel2Payload)
            throw new SerializationException($"Payload is not of type {nameof(AcceptChannel2Payload)}");
        
        // Get the value object serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(acceptChannel2Payload.TemporaryChannelId, stream);
        
        // Serialize other types
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian((ulong)acceptChannel2Payload.FundingAmount.Satoshi));
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian((ulong)acceptChannel2Payload.DustLimitAmount.Satoshi));
        await stream
            .WriteAsync(EndianBitConverter
                .GetBytesBigEndian(acceptChannel2Payload.MaxHtlcValueInFlightAmount.MilliSatoshi));
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel2Payload.HtlcMinimumAmount.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel2Payload.MinimumDepth));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel2Payload.ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(acceptChannel2Payload.MaxAcceptedHtlcs));
        await stream.WriteAsync(acceptChannel2Payload.FundingPubKey.ToBytes());
        await stream.WriteAsync(acceptChannel2Payload.RevocationBasepoint.ToBytes());
        await stream.WriteAsync(acceptChannel2Payload.PaymentBasepoint.ToBytes());
        await stream.WriteAsync(acceptChannel2Payload.DelayedPaymentBasepoint.ToBytes());
        await stream.WriteAsync(acceptChannel2Payload.HtlcBasepoint.ToBytes());
        await stream.WriteAsync(acceptChannel2Payload.FirstPerCommitmentPoint.ToBytes());
    }

    public async Task<AcceptChannel2Payload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.PUBKEY_LEN);
        
        try
        {
            // Get the value object serializer
            var channelIdSerializer = 
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var temporaryChannelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var fundingSatoshis = LightningMoney.FromUnit(EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]),
                                                          LightningMoneyUnit.Satoshi);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var dustLimitSatoshis = 
                LightningMoney.FromUnit(EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]),
                                        LightningMoneyUnit.Satoshi);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var maxHtlcValueInFlightMsat = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var htlcMinimumMsat = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var minimumDepth = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var fundingPubKey = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var revocationBasepoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var paymentBasepoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var delayedPaymentBasepoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var htlcBasepoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var firstPerCommitmentPoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);

            return new AcceptChannel2Payload(delayedPaymentBasepoint, dustLimitSatoshis, firstPerCommitmentPoint,
                                             fundingSatoshis, fundingPubKey, htlcBasepoint, htlcMinimumMsat,
                                             maxAcceptedHtlcs, maxHtlcValueInFlightMsat, minimumDepth, paymentBasepoint,
                                             revocationBasepoint, temporaryChannelId, toSelfDelay);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing AcceptChannel2Payload", e);
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