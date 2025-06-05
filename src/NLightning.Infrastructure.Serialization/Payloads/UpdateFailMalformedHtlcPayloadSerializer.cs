using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Crypto.Constants;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Exceptions;

public class UpdateFailMalformedHtlcPayloadSerializer : IPayloadSerializer<UpdateFailMalformedHtlcPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public UpdateFailMalformedHtlcPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not UpdateFailMalformedHtlcPayload updateFailMalformedHtlcPayload)
            throw new SerializationException($"Payload is not of type {nameof(UpdateFailMalformedHtlcPayload)}");

        // Get the ChannelId serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(updateFailMalformedHtlcPayload.ChannelId, stream);

        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(updateFailMalformedHtlcPayload.Id));
        await stream.WriteAsync(updateFailMalformedHtlcPayload.Sha256OfOnion);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(updateFailMalformedHtlcPayload.FailureCode));
    }

    public async Task<UpdateFailMalformedHtlcPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ulong));

        try
        {
            // Get the ChannelId serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ulong)]);
            var id = EndianBitConverter.ToUInt64BigEndian(buffer[..sizeof(ulong)]);

            var sha256OfOnion = new byte[CryptoConstants.Sha256HashLen];
            await stream.ReadExactlyAsync(sha256OfOnion);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var failureCode = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            return new UpdateFailMalformedHtlcPayload(channelId, failureCode, id, sha256OfOnion);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(UpdateFailMalformedHtlcPayload)}", e);
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