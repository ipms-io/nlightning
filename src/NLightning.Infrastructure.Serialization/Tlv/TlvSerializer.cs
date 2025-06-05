using System.Buffers;
using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Domain.Serialization.Interfaces;

namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Protocol.Tlv;

public class TlvSerializer : ITlvSerializer
{
    private readonly IValueObjectTypeSerializer<BigSize> _bigSizeSerializer;

    public TlvSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _bigSizeSerializer = valueObjectSerializerFactory.GetSerializer<BigSize>()
                          ?? throw new ArgumentNullException(nameof(valueObjectSerializerFactory));
    }

    /// <summary>
    /// Serializes a BaseTlv value into a stream.
    /// </summary>
    /// <param name="baseTlv">The BaseTlv value to serialize.</param>
    /// <param name="stream">The stream where the serialized value will be written.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the stream is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the write operation.</exception>
    public async Task SerializeAsync(BaseTlv baseTlv, Stream stream)
    {
        await _bigSizeSerializer.SerializeAsync(baseTlv.Type, stream);
        await _bigSizeSerializer.SerializeAsync(baseTlv.Length, stream);

        await stream.WriteAsync(baseTlv.Value);
    }

    /// <summary>
    /// Deserializes a BaseTlv value from a stream.
    /// </summary>
    /// <param name="stream">The stream from which the BaseTlv value will be deserialized.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized BaseTlv value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or contains insufficient data for deserialization.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the read operation.</exception>
    public async Task<BaseTlv?> DeserializeAsync(Stream stream)
    {
        if (stream.Position == stream.Length)
            return null;

        byte[]? value = null;

        try
        {
            var type = await _bigSizeSerializer.DeserializeAsync(stream);
            var length = await _bigSizeSerializer.DeserializeAsync(stream);

            value = ArrayPool<byte>.Shared.Rent(length);
            await stream.ReadExactlyAsync(value.AsMemory()[..(int)length]);

            return new BaseTlv(type, length, value[..(int)length]);
        }
        finally
        {
            if (value is not null)
                ArrayPool<byte>.Shared.Return(value);
        }
    }
}