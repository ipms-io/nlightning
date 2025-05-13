using System.Buffers;
using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.ValueObjects;

using Crypto.Constants;
using Domain.ValueObjects;
using Domain.ValueObjects.Interfaces;
using Interfaces;

public class WitnessSerializer : IValueObjectSerializer<Witness>
{
    /// <summary>
    /// Serializes a Witness value into a stream.
    /// </summary>
    /// <param name="valueObject">The Witness value to serialize.</param>
    /// <param name="stream">The stream where the serialized value will be written.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the stream is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the write operation.</exception>
    public async Task SerializeAsync(IValueObject valueObject, Stream stream)
    {
        if (valueObject is not Witness witness)
            throw new ArgumentException("Value object must be of type Witness.", nameof(valueObject));
        
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(witness.Length));
        await stream.WriteAsync(witness);
    }

    /// <summary>
    /// Deserializes a Witness value from a stream.
    /// </summary>
    /// <param name="stream">The stream from which the Witness value will be deserialized.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized Witness value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or contains insufficient data for deserialization.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the read operation.</exception>
    public async Task<Witness> DeserializeAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(CryptoConstants.MAX_SIGNATURE_SIZE);
        
        try
        {
            await stream.ReadExactlyAsync(buffer.AsMemory()[..2]);
            var length = EndianBitConverter.ToUInt16BigEndian(buffer[..2]);

            if (length > CryptoConstants.MAX_SIGNATURE_SIZE)
                throw new SerializationException($"Witness length {length} exceeds maximum size of {CryptoConstants.MAX_SIGNATURE_SIZE} bytes.");
            
            await stream.ReadExactlyAsync(buffer.AsMemory()[..length]);

            return new Witness(buffer[..length]);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    async Task<IValueObject> IValueObjectSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}