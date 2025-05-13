namespace NLightning.Domain.Protocol.Models;

using ValueObjects;

public class BaseTlv
{
    /// <summary>
    /// The type of the TLV
    /// </summary>
    public BigSize Type { get; }

    /// <summary>
    /// The length of the value
    /// </summary>
    public BigSize Length { get; protected init; }

    /// <summary>
    /// The value
    /// </summary>
    public byte[] Value { get; internal set; }

    public BaseTlv(BigSize type, BigSize length, byte[] value)
    {
        Type = type;
        Length = length;
        Value = value;
    }

    /// <summary>
    /// Create a new TLV
    /// </summary>
    /// <param name="type">The type of the TLV</param>
    /// <param name="value">The value of the TLV</param>
    /// <remarks>
    /// The length of the value is automatically calculated
    /// </remarks>
    public BaseTlv(BigSize type, byte[] value) : this(type, value.Length, value)
    { }

    /// <summary>
    /// Create a new TLV
    /// </summary>
    /// <param name="type">The type of the TLV</param>
    /// <remarks>
    /// Used internally to create a TLV with a length of 0 and an empty value.
    /// </remarks>
    internal BaseTlv(BigSize type) : this(type, [])
    { }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is BaseTlv tlv && Equals(tlv);
    }

    public bool Equals(BaseTlv other)
    {
        return Type.Equals(other.Type) && Length.Equals(other.Length) && Value.Equals(other.Value);
    }

    public static bool operator ==(BaseTlv left, BaseTlv right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BaseTlv left, BaseTlv right)
    {
        return !(left == right);
    }
}