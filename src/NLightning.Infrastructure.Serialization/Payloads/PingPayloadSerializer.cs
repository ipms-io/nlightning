using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Exceptions;

public class PingPayloadSerializer : IPayloadSerializer<PingPayload>
{
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not PingPayload pingPayload)
            throw new SerializationException($"Payload is not of type {nameof(PingPayload)}");
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(pingPayload.NumPongBytes));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(pingPayload.BytesLength));
        await stream.WriteAsync(pingPayload.Ignored);
    }

    public async Task<PingPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ushort));

        try
        {
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var numPongBytes = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var bytesLength = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            if (stream.Length - stream.Position < bytesLength)
                throw new SerializationException(
                    $"Invalid Ignored data for PingPayload. Expected {bytesLength} bytes.");

            return new PingPayload
            {
                NumPongBytes = numPongBytes,
                BytesLength = bytesLength
            };
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing PingPayload", e);
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