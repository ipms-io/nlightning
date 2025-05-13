using System.Buffers;

namespace NLightning.Infrastructure.Serialization.ValueObjects;

using Domain.ValueObjects;
using Domain.ValueObjects.Interfaces;
using Interfaces;

public class BigSizeSerializer : IValueObjectSerializer<BigSize>
{
    /// <summary>
    /// Serializes a BigSize value into a stream.
    /// </summary>
    /// <param name="valueObject">The BigSize value to serialize.</param>
    /// <param name="stream">The stream where the serialized value will be written.</param>
    /// <returns>A task that represents the asynchronous serialization operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the stream is null.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the write operation.</exception>
    public async Task SerializeAsync(IValueObject valueObject, Stream stream)
    {
        if (valueObject is not BigSize bigSize)
            throw new ArgumentException("Value object must be of type BigSize.", nameof(valueObject));
        
        if (bigSize < 0xfd)
        {
            await stream.WriteAsync(new[] { (byte)bigSize });
        }
        else if (bigSize < 0x10000)
        {
            await stream.WriteAsync(new byte[] { 0xfd });
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((ushort)bigSize));
        }
        else if (bigSize < 0x100000000)
        {
            await stream.WriteAsync(new byte[] { 0xfe });
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian((uint)bigSize));
        }
        else
        {
            await stream.WriteAsync(new byte[] { 0xff });
            await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(bigSize));
        }
    }

    /// <summary>
    /// Deserializes a BigSize value from a stream.
    /// </summary>
    /// <param name="stream">The stream from which the BigSize value will be deserialized.</param>
    /// <returns>A task that represents the asynchronous deserialization operation, containing the deserialized BigSize value.</returns>
    /// <exception cref="ArgumentException">Thrown when the stream is empty or contains insufficient data for deserialization.</exception>
    /// <exception cref="IOException">Thrown when an I/O error occurs during the read operation.</exception>
    public async Task<BigSize> DeserializeAsync(Stream stream)
    {
        if (stream.Position == stream.Length)
            throw new ArgumentException("BigSize cannot be read from an empty stream.");

        var prefix = ArrayPool<byte>.Shared.Rent(1);
        
        try
        {
            await stream.ReadExactlyAsync(prefix.AsMemory()[..1]);
            ulong value;

            switch (prefix[0])
            {
                case < 0xfd:
                    value = prefix[0];
                    break;
                // Check if there are enough bytes to read
                case 0xfd when stream.Position + 2 > stream.Length:
                    throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
                case 0xfd:
                {
                    var bytes = new byte[2];
                    await stream.ReadExactlyAsync(bytes);
                    value = EndianBitConverter.ToUInt16BigEndian(bytes);
                    break;
                }
                case 0xfe when stream.Position + 4 > stream.Length:
                    throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
                case 0xfe:
                {
                    var bytes = new byte[4];
                    await stream.ReadExactlyAsync(bytes);
                    value = EndianBitConverter.ToUInt32BigEndian(bytes);
                    break;
                }
                default:
                {
                    if (stream.Position + 8 > stream.Length)
                    {
                        throw new ArgumentException("BigSize cannot be read from a stream with insufficient data.");
                    }

                    var bytes = new byte[8];
                    await stream.ReadExactlyAsync(bytes);
                    value = EndianBitConverter.ToUInt64BigEndian(bytes);
                    break;
                }
            }

            return new BigSize(value);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(prefix);
        }
    }
    async Task<IValueObject> IValueObjectSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}