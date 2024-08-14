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
    public int Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExpiryTimeTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Expiry Time in seconds</param>
    public ExpiryTimeTaggedField(int value)
    {
        Value = value;
        // Calculate the length of the field by getting the number of bits needed to represent the value plus 1
        // then add 4 to round up to the next multiple of 5 and divide by 5 to get the number of bytes
        Length = (short)((BitOperations.Log2((uint)Value) + 1 + 4) / 5);
    }

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

    public object GetValue()
    {
        return Value;
    }

    public static ExpiryTimeTaggedField FromBitReader(BitReader bitReader, short length)
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