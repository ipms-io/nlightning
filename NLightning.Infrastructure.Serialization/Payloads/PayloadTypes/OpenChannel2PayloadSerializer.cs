using System.Buffers;
using System.Runtime.Serialization;
using NBitcoin;
using NLightning.Domain.Crypto.Constants;
using NLightning.Infrastructure.Converters;

namespace NLightning.Infrastructure.Serialization.Payloads.PayloadTypes;

using Domain.Enums;
using Domain.Money;
using Domain.ValueObjects;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Factories;
using Domain.Serialization.Payloads;
using Exceptions;

public class OpenChannel2PayloadSerializer : IPayloadTypeSerializer<OpenChannel2Payload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public OpenChannel2PayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not OpenChannel2Payload openChannel2Payload)
            throw new SerializationException($"Payload is not of type {nameof(OpenChannel2Payload)}");
        
        // Get the ChainHash serializer
        var chainHashSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChainHash>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChainHash)}");
        await chainHashSerializer.SerializeAsync(openChannel2Payload.ChainHash, stream);
        
        // Get the ChannelId serializer
        var channelIdSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelId>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(openChannel2Payload.TemporaryChannelId, stream);
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.FundingFeeRatePerKw));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.CommitmentFeeRatePerKw));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.FundingAmount.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.DustLimitAmount.Satoshi));
        await stream.WriteAsync(
            EndianBitConverter.GetBytesBigEndian(openChannel2Payload.MaxHtlcValueInFlightAmount.MilliSatoshi));
        await stream
            .WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.HtlcMinimumAmount.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.MaxAcceptedHtlcs));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(openChannel2Payload.Locktime));
        await stream.WriteAsync(openChannel2Payload.FundingPubKey.ToBytes());
        await stream.WriteAsync(openChannel2Payload.RevocationBasepoint.ToBytes());
        await stream.WriteAsync(openChannel2Payload.PaymentBasepoint.ToBytes());
        await stream.WriteAsync(openChannel2Payload.DelayedPaymentBasepoint.ToBytes());
        await stream.WriteAsync(openChannel2Payload.HtlcBasepoint.ToBytes());
        await stream.WriteAsync(openChannel2Payload.FirstPerCommitmentPoint.ToBytes());
        await stream.WriteAsync(openChannel2Payload.SecondPerCommitmentPoint.ToBytes());
        
        // Get the ChannelFlags serializer
        var channelFlagsSerializer = 
            _valueObjectSerializerFactory.GetSerializer<ChannelFlags>() 
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelFlags)}");
        await channelFlagsSerializer.SerializeAsync(openChannel2Payload.ChannelFlags, stream);
    }

    public async Task<OpenChannel2Payload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.PUBKEY_LEN);

        try
        {
            // Get the ChainHash serializer
            var chainHashSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChainHash>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChainHash)}");
            var chainHash = await chainHashSerializer.DeserializeAsync(stream);

            // Get the ChannelId serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var temporaryChannelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var fundingFeeRatePerKw = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var commitmentFeeRatePerKw = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var fundingSatoshis = LightningMoney.FromUnit(EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]),
                                                          LightningMoneyUnit.Satoshi);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var dustLimitSatoshis = LightningMoney
                .FromUnit(EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]), LightningMoneyUnit.Satoshi);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var maxHtlcValueInFlightMsat = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var htlcMinimumMsat = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(uint)]);
            var locktime = EndianBitConverter.ToUInt32BigEndian(buffer[..sizeof(uint)]);

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

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.PUBKEY_LEN]);
            var secondPerCommitmentPoint = new PubKey(buffer[..CryptoConstants.PUBKEY_LEN]);
            
            // Get the ChannelFlags serializer
            var channelFlagsSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelFlags>()
                ?? throw new SerializationException(
                    $"No serializer found for value object type {nameof(ChannelFlags)}");
            var channelFlags = await channelFlagsSerializer.DeserializeAsync(stream);

            return new OpenChannel2Payload(chainHash, channelFlags, commitmentFeeRatePerKw, delayedPaymentBasepoint,
                                           dustLimitSatoshis, firstPerCommitmentPoint, fundingSatoshis,
                                           fundingFeeRatePerKw, fundingPubKey, htlcBasepoint, htlcMinimumMsat, locktime,
                                           maxAcceptedHtlcs, maxHtlcValueInFlightMsat, paymentBasepoint,
                                           revocationBasepoint, secondPerCommitmentPoint, toSelfDelay,
                                           temporaryChannelId);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing OpenChannel2Payload", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    async Task<IMessagePayload?> IPayloadTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}