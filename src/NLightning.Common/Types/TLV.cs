using System.Runtime.Serialization;

namespace NLightning.Common.Types;

/// <summary>
/// A TLV (Type-Length-Value) object
/// </summary>
/// <param name="type">A message-specific, 64-bit identifier for the TLV</param>
/// <param name="length">The length of the value</param>
/// <param name="value">The value</param>
public class Tlv(BigSize type, BigSize length, byte[] value) : IEquatable<Tlv>
{
    /// <summary>
    /// The type of the TLV
    /// </summary>
    public BigSize Type => type;

    /// <summary>
    /// The length of the value
    /// </summary>
    public BigSize Length { get; set; } = length;

    /// <summary>
    /// The value
    /// </summary>
    public byte[] Value { get; set; } = value;

    /// <summary>
    /// Create a new TLV
    /// </summary>
    /// <param name="type">The type of the TLV</param>
    /// <param name="value">The value of the TLV</param>
    /// <remarks>
    /// The length of the value is automatically calculated
    /// </remarks>
    public Tlv(BigSize type, byte[] value) : this(type, value.Length, value)
    { }

    /// <summary>
    /// Create a new TLV
    /// </summary>
    /// <param name="type">The type of the TLV</param>
    /// <remarks>
    /// Used internally to create a TLV with a length of 0 and an empty value.
    /// </remarks>
    internal Tlv(BigSize type) : this(type, 0, [])
    { }

    /// <summary>
    /// Serialize the TLV to a stream
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="SerializationException">Error serializing TLV or any of it's parts</exception>
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
    /// <param name="stream">The stream to use</param>
    /// <returns>The TLV</returns>
    /// <exception cref="SerializationException">Error deserializing TLV or any of it's parts</exception>
    public static async Task<Tlv> DeserializeAsync(Stream stream)
    {
        try
        {
            var type = await BigSize.DeserializeAsync(stream);
            var length = await BigSize.DeserializeAsync(stream);
            var value = new byte[length];
            await stream.ReadExactlyAsync(value);

            return new Tlv(type, length, value);
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
    public bool Equals(Tlv? other)
    {
        return other?.Type == Type;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Tlv tlv && Equals(tlv);
    }
}