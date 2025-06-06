using System.Buffers;
using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Bitcoin.Constants;
using Domain.Bitcoin.ValueObjects;
using Domain.Channels.ValueObjects;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Domain.Serialization.Interfaces;
using Exceptions;

public class ShutdownPayloadSerializer : IPayloadSerializer<ShutdownPayload>
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public ShutdownPayloadSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
    }

    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not ShutdownPayload shutdownPayload)
            throw new SerializationException($"Payload is not of type {nameof(ShutdownPayload)}");

        // Get the value object serializer
        var channelIdSerializer =
            _valueObjectSerializerFactory.GetSerializer<ChannelId>()
            ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
        await channelIdSerializer.SerializeAsync(shutdownPayload.ChannelId, stream);

        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(shutdownPayload.ScriptPubkeyLen));
        await stream.WriteAsync(shutdownPayload.ScriptPubkey);
    }

    public async Task<ShutdownPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(ScriptConstants.MaxScriptSize);

        try
        {
            // Get the value object serializer
            var channelIdSerializer =
                _valueObjectSerializerFactory.GetSerializer<ChannelId>()
                ?? throw new SerializationException($"No serializer found for value object type {nameof(ChannelId)}");
            var channelId = await channelIdSerializer.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var len = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            if (len > ScriptConstants.MaxScriptSize)
                throw new SerializationException(
                    $"ScriptPubkey length {len} exceeds maximum size {ScriptConstants.MaxScriptSize}");

            await stream.ReadExactlyAsync(buffer.AsMemory()[..len]);
            var scriptPubkey = new BitcoinScript(buffer[..len]);

            return new ShutdownPayload(channelId, scriptPubkey);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(ShutdownPayload)}", e);
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