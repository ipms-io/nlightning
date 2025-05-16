using System.Buffers;
using NLightning.Domain.Serialization.ValueObjects;
using NLightning.Domain.ValueObjects;
using NLightning.Domain.ValueObjects.Interfaces;

namespace NLightning.Infrastructure.Serialization.ValueObjects;

public class ChainHashTypeSerializer : IValueObjectTypeSerializer<ChainHash>
{
    /// <summary>
    /// Serializes a ChainHash value into a stream.
    /// </summary>
    /// <param name="valueObject">The ChainHash value to serialize.</param>
    /// <param name="stream">The stream where the serialized value will be written.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the stream is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the write operation.</exception>
    public async Task SerializeAsync(IValueObject valueObject, Stream stream)
    {
        if (valueObject is not ChainHash chainHash)
            throw new ArgumentException("Value object must be of type ChainHash.", nameof(valueObject));
        
        await stream.WriteAsync(chainHash);
    }

    /// <summary>
    /// Deserializes a ChainHash value from a stream.
    /// </summary>
    /// <param name="stream">The stream from which the ChainHash value will be deserialized.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized ChainHash value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or contains insufficient data for deserialization.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the read operation.</exception>
    public async Task<ChainHash> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(ChainHash.LENGTH);
        
        try
        {
            await stream.ReadExactlyAsync(buffer.AsMemory()[..ChainHash.LENGTH]);

            return new ChainHash(buffer);
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