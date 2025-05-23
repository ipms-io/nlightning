using System.Buffers;

namespace NLightning.Infrastructure.Serialization.ValueObjects;

using Domain.Serialization.ValueObjects;
using Domain.ValueObjects;
using Domain.ValueObjects.Interfaces;

public class ChannelIdTypeSerializer : IValueObjectTypeSerializer<ChannelId>
{
    /// <summary>
    /// Serializes a ChannelId value into a stream.
    /// </summary>
    /// <param name="valueObject">The ChannelId value to serialize.</param>
    /// <param name="stream">The stream where the serialized value will be written.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the stream is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the write operation.</exception>
    public async Task SerializeAsync(IValueObject valueObject, Stream stream)
    {
        if (valueObject is not ChannelId channelId)
            throw new ArgumentException("Value object must be of type ChannelId.", nameof(valueObject));

        await stream.WriteAsync(channelId);
    }

    /// <summary>
    /// Deserializes a ChannelId value from a stream.
    /// </summary>
    /// <param name="stream">The stream from which the ChannelId value will be deserialized.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized ChannelId value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or contains insufficient data for deserialization.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the read operation.</exception>
    public async Task<ChannelId> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(ChannelId.LENGTH);

        try
        {
            await stream.ReadExactlyAsync(buffer.AsMemory()[..ChannelId.LENGTH]);

            return new ChannelId(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    async Task<IValueObject> IValueObjectTypeSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}