using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.ValueObjects;
using Converters;
using Domain.Crypto.Constants;
using Domain.Enums;
using Domain.Money;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.ValueObjects;
using Exceptions;

public class OpenChannel2PayloadSerializer : IPayloadSerializer<OpenChannel2Payload>
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
        await channelIdSerializer.SerializeAsync(openChannel2Payload.ChannelId, stream);

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
        await stream.WriteAsync(openChannel2Payload.FundingPubKey);
        await stream.WriteAsync(openChannel2Payload.RevocationBasepoint);
        await stream.WriteAsync(openChannel2Payload.PaymentBasepoint);
        await stream.WriteAsync(openChannel2Payload.DelayedPaymentBasepoint);
        await stream.WriteAsync(openChannel2Payload.HtlcBasepoint);
        await stream.WriteAsync(openChannel2Payload.FirstPerCommitmentPoint);
        await stream.WriteAsync(openChannel2Payload.SecondPerCommitmentPoint);

        // Get the ChannelFlags serializer
        var channelFlagsSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelFlags>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelFlags)}");
        await channelFlagsSerializer.SerializeAsync(openChannel2Payload.ChannelFlags, stream);
    }

    public async Task<OpenChannel2Payload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.CompactPubkeyLen);

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

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var secondPerCommitmentPoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

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

    async Task<IMessagePayload?> IPayloadSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}