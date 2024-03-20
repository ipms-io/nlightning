using System.Runtime.Serialization;

namespace NLightning.Common.Types;

/// <summary>
/// A TLV (Type-Length-Value) object
/// </summary>
/// <param name="type">A message-specific, 64-bit identifier for the TLV</param>
/// <param name="length">The length of the value</param>
/// <param name="value">The value</param>
public sealed class TLV(BigSize type, BigSize length, byte[] value) : IEquatable<TLV>
{
    public BigSize Type { get; set; } = type;
    public BigSize Length { get; set; } = length;
    public byte[] Value { get; set; } = value;

    public TLV(BigSize type, byte[] value) : this(type, value.Length, value)
    { }

    public byte[] Serialize()
    {
        try
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            Type.Serialize(writer);
            Length.Serialize(writer);
            writer.Write(Value);

            return stream.ToArray();
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
    public static TLV Deserialize(BinaryReader reader)
    {
        try
        {
            var type = BigSize.Deserialize(reader);
            var length = BigSize.Deserialize(reader);
            var value = reader.ReadBytes(length);
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