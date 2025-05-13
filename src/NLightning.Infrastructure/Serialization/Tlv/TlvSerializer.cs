using System.Buffers;
using System.Runtime.Serialization;

namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Protocol.Models;
using Domain.ValueObjects;
using Interfaces;

public class TlvSerializer : ITlvSerializer
{
    private readonly IValueObjectSerializerFactory _valueObjectSerializerFactory;

    public TlvSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
    {
        _valueObjectSerializerFactory = valueObjectSerializerFactory;
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
        await _valueObjectSerializerFactory.SerializeAsync(baseTlv.Type, stream);
        await _valueObjectSerializerFactory.SerializeAsync(baseTlv.Length, stream);

        await stream.WriteAsync(baseTlv.Value);
    }

    /// <summary>
    /// Deserializes a BaseTlv value from a stream.
    /// </summary>
    /// <param name="stream">The stream from which the BaseTlv value will be deserialized.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized BaseTlv value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or contains insufficient data for deserialization.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the read operation.</exception>
    public async Task<TlvType> DeserializeAsync<TlvType>(Stream stream) where TlvType : BaseTlv
    {
        if (stream.Position == stream.Length)
            throw new ArgumentException("BaseTlv cannot be read from an empty stream.");

        byte[]? value = null;
        
        try
        {
            var type = await _valueObjectSerializerFactory.DeserializeAsync<BigSize>(stream);
            var length = await _valueObjectSerializerFactory.DeserializeAsync<BigSize>(stream);
            
            value = ArrayPool<byte>.Shared.Rent(length);
            await stream.ReadExactlyAsync(value.AsMemory()[..(int)length]);

            return new BaseTlv(type, length, value[..(int)length]) as TlvType
                   ?? throw new SerializationException($"Unable to box BaseTlv to {typeof(TlvType).Name}");
        }
        finally
        {
            if (value is not null)
                ArrayPool<byte>.Shared.Return(value);
        }
    }
}