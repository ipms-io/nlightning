using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Channels.ValueObjects;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.Payloads;
using Exceptions;

public class ChannelReestablishPayloadSerializer : IPayloadSerializer<ChannelReestablishPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ChannelReestablishPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ChannelReestablishPayload channelReestablishPayload)
            throw new SerializationException($"Payload is not of type {nameof(ChannelReestablishPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
         ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(channelReestablishPayload.ChannelId, stream);

        // Serialize other types
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(channelReestablishPayload.NextCommitmentNumber));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(channelReestablishPayload.NextRevocationNumber));
        await stream.WriteAsync(channelReestablishPayload.YourLastPerCommitmentSecret);
        await stream.WriteAsync(channelReestablishPayload.MyCurrentPerCommitmentPoint);
    }

    public async Task<ChannelReestablishPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.CompactPubkeyLen);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
             ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var nextCommitmentNumber = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var nextRevocationNumber = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            var yourLastPerCommitmentSecret = new byte[32];
            await stream.ReadExactlyAsync(yourLastPerCommitmentSecret);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..CryptoConstants.CompactPubkeyLen]);
            var myCurrentPerCommitmentPoint = new CompactPubKey(buffer[..CryptoConstants.CompactPubkeyLen]);

            return new ChannelReestablishPayload(channelId, myCurrentPerCommitmentPoint, nextCommitmentNumber,
                                                 nextRevocationNumber, yourLastPerCommitmentSecret);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing ChannelReestablishPayload", e);
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