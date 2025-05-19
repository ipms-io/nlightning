using System.Buffers;
using System.Runtime.Serialization;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Converters;
using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Exceptions;

public class PongPayloadSerializer : IPayloadSerializer<PongPayload>
{
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not PongPayload pongPayload)
            throw new SerializationException($"Payload is not of type {nameof(PongPayload)}");
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(pongPayload.BytesLength));
        await stream.WriteAsync(pongPayload.Ignored);
    }

    public async Task<PongPayload?> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(sizeof(ushort));

        try
        {
            await stream.ReadExactlyAsync(buffer.AsMemory()[..sizeof(ushort)]);
            var bytesLength = EndianBitConverter.ToUInt16BigEndian(buffer[..sizeof(ushort)]);

            if (stream.Length - stream.Position < bytesLength)
                throw new SerializationException(
                    $"Invalid Ignored data for {nameof(PongPayload)}. Expected {bytesLength} bytes.");

            return new PongPayload(bytesLength);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException($"Error deserializing {nameof(PongPayload)}", e);
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