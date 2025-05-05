using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Crypto.Constants;
using Domain.Money;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Domain.ValueObjects;
using Exceptions;

public class OpenChannel1PayloadSerializer : IPayloadSerializer<OpenChannel1Payload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public OpenChannel1PayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not OpenChannel1Payload openChannel1Payload)
            throw new SerializationException($"Payload is not of type {nameof(OpenChannel1Payload)}");

        // Get the value object serializer
        var chainHashSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChainHash>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChainHash)}");
        await chainHashSerializer.SerializeAsync(openChannel1Payload.ChainHash, stream);
        
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(openChannel1Payload.ChannelId, stream);
        
        var channelFlagsSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelFlags>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelFlags)}");

        // Serialize other types
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian((ulong)openChannel1Payload.FundingAmount.Satoshi));
        await stream
            .WriteAsync(EndianBitConverter
                .GetBytesBigEndian(openChannel1Payload.PushAmount.MilliSatoshi));
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian((ulong)openChannel1Payload.DustLimitAmount.Satoshi));
        await stream
            .WriteAsync(EndianBitConverter
                .GetBytesBigEndian(openChannel1Payload.MaxHtlcValueInFlight.MilliSatoshi));
        await stream
            .WriteAsync(EndianBitConverter
                .GetBytesBigEndian((ulong)openChannel1Payload.ChannelReserveAmount.Satoshi));
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel1Payload.HtlcMinimumAmount.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ulong)openChannel1Payload.FeeRatePerKw.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel1Payload.ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel1Payload.MaxAcceptedHtlcs));
        await stream.WriteAsync(openChannel1Payload.FundingPubKey.ToBytes());
        await stream.WriteAsync(openChannel1Payload.RevocationBasepoint.ToBytes());
        await stream.WriteAsync(openChannel1Payload.PaymentBasepoint.ToBytes());
        await stream.WriteAsync(openChannel1Payload.DelayedPaymentBasepoint.ToBytes());
        await stream.WriteAsync(openChannel1Payload.HtlcBasepoint.ToBytes());
        await stream.WriteAsync(openChannel1Payload.FirstPerCommitmentPoint.ToBytes());
        
        await channelFlagsSerializer.SerializeAsync(openChannel1Payload.ChannelFlags, stream);
    }

    public async Task<OpenChannel1Payload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.PUBKEY_LEN);

        try
        {
            // Get the value object serializer
            var chainHashSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChainHash>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChainHash)}");
            var chainHash = await chainHashSerializer.DeserializeAsync(stream);
            
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var temporaryChannelId = await channelIdSerializer.DeserializeAsync(stream);
        
            var channelFlagsSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelFlags>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelFlags)}");

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var fundingSatoshis = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var pushAmount = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var dustLimitSatoshis = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var maxHtlcValueInFlightMsat = LightningMoney
                .MilliSatoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var channelReserveAmount = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var htlcMinimumAmount = LightningMoney
                .MilliSatoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var feeRatePerKw = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(buffer.AsSpan()[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(buffer.AsSpan()[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(buffer.AsSpan()[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var fundingPubKey = new PubKey(buffer.AsSpan()[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var revocationBasepoint = new PubKey(buffer.AsSpan()[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var paymentBasepoint = new PubKey(buffer.AsSpan()[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var delayedPaymentBasepoint = new PubKey(buffer.AsSpan()[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var htlcBasepoint = new PubKey(buffer.AsSpan()[..CryptoConstants.PUBKEY_LEN]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var firstPerCommitmentPoint = new PubKey(buffer.AsSpan()[..CryptoConstants.PUBKEY_LEN]);

            var channelFlags = await channelFlagsSerializer.DeserializeAsync(stream);
            
            return new OpenChannel1Payload(chainHash, temporaryChannelId, fundingSatoshis, pushAmount,
                                           dustLimitSatoshis, maxHtlcValueInFlightMsat, channelReserveAmount,
                                           htlcMinimumAmount, feeRatePerKw, toSelfDelay, maxAcceptedHtlcs,
                                           fundingPubKey, revocationBasepoint, paymentBasepoint,
                                           delayedPaymentBasepoint, htlcBasepoint, firstPerCommitmentPoint,
                                           channelFlags);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing OpenChannel1Payload", e);
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