using NLightning.Bolts.BOLT11.Interfaces;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

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
        var data = EndianBitConverter.GetBytesBigEndian(value, true);
        Length = (short)((data.Length * 8 - 7) / 5);
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write type
        bitWriter.WriteByteAsBits((byte)Type, 5);

        // Write length
        bitWriter.WriteInt16AsBits(Length, 10);

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