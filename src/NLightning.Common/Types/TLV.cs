using System.Runtime.Serialization;

namespace NLightning.Common.Types;

/// <summary>
/// A TLV (Type-Length-Value) object
/// </summary>
/// <param name="type">A message-specific, 64-bit identifier for the TLV</param>
/// <param name="length">The length of the value</param>
/// <param name="value">The value</param>
public class TLV(BigSize type, BigSize length, byte[] value) : IEquatable<TLV>
{
    public BigSize Type { get; set; } = type;
    public BigSize Length { get; set; } = length;
    public byte[] Value { get; set; } = value;

    public TLV(BigSize type, byte[] value) : this(type, value.Length, value)
    { }
    internal TLV(BigSize type) : this(type, 0, [])
    { }

    public async Task SerializeAsync(Stream stream)
    {
        try
        {
            await Type.SerializeAsync(stream);
            await Length.SerializeAsync(stream);

            await stream.WriteAsync(Value);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error serializing TLV", e);
        }
    }

    /// <summary>
    /// Deserialize a TLV from a stream
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <returns>The TLV</returns>
    /// <exception cref="SerializationException">Error deserializing TLV or any of it's parts</exception>
    public static async Task<TLV> DeserializeAsync(Stream stream)
    {
        try
        {
            var type = await BigSize.DeserializeAsync(stream);
            var length = await BigSize.DeserializeAsync(stream);
            var value = new byte[length];
            await stream.ReadExactlyAsync(value);

            return new TLV(type, length, value);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TLV", e);
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    /// <summary>
    /// Check if two TLVs are equal
    /// </summary>
    /// <param name="other">The other TLV</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public bool Equals(TLV? other)
    {
        return other?.Type == Type;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TLV tlv && Equals(tlv);
    }
}