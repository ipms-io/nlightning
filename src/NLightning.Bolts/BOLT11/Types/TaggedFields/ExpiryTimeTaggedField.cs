using System.Numerics;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the expiry time
/// </summary>
/// <remarks>
/// The expiry time is the time in seconds after which the invoice is invalid.
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class ExpiryTimeTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.EXPIRY_TIME;
    internal int Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiryTimeTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Expiry Time in seconds</param>
    internal ExpiryTimeTaggedField(int value)
    {
        Value = value;
        // Calculate the length of the field by getting the number of bits needed to represent the value plus 1
        // then add 4 to round up to the next multiple of 5 and divide by 5 to get the number of bytes
        Length = (short)((BitOperations.Log2((uint)Value) + 1 + 4) / 5);
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write data
        bitWriter.WriteInt32AsBits(Value, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return Value > 0;
    }

    /// <summary>
    /// Reads a ExpiryTimeTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The ExpiryTimeTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static ExpiryTimeTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Invalid length for ExpiryTimeTaggedField. Length must be greater than 0", nameof(length));
        }

        // Read the data from the BitReader
        var value = bitReader.ReadInt32FromBits(length * 5);

        return new ExpiryTimeTaggedField(value);
    }
}