using NLightning.Bolts.BOLT11.Interfaces;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

/// <summary>
/// Tagged field for the minimum final cltv expiry
/// </summary>
/// <remarks>
/// The minimum final cltv expiry is a 4 byte field that specifies the minimum number of blocks that the receiver should wait to claim the payment
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class MinFinalCltvExpiryTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.MIN_FINAL_CLTV_EXPIRY;
    public ushort Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinFinalCltvExpiryTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Expiry Time in seconds</param>
    public MinFinalCltvExpiryTaggedField(ushort value)
    {
        Value = value;
        var data = EndianBitConverter.GetBytesBigEndian(value, true);
        Length = (short)((data.Length * 8 - 7) / 5);
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write data
        bitWriter.WriteUInt16AsBits(Value, Length * 5);
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

    public static MinFinalCltvExpiryTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length <= 0)
        {
            throw new ArgumentException("Invalid length for MinFinalCltvExpiryTaggedField. Length must be greater than 0", nameof(length));
        }

        // Read the data from the BitReader
        var value = bitReader.ReadUInt16FromBits(length * 5);

        return new MinFinalCltvExpiryTaggedField(value);
    }
}