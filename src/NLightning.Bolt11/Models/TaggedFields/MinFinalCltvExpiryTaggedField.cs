using System.Numerics;
using NLightning.Domain.Utils;

namespace NLightning.Bolt11.Models.TaggedFields;

using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the minimum final cltv expiry
/// </summary>
/// <remarks>
/// The minimum final cltv expiry is a 4 byte field that specifies the minimum number of blocks that the receiver should wait to claim the payment
/// </remarks>
/// <seealso cref="ITaggedField"/>
internal sealed class MinFinalCltvExpiryTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.MinFinalCltvExpiry;
    internal ushort Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinFinalCltvExpiryTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Expiry Time in seconds</param>
    internal MinFinalCltvExpiryTaggedField(ushort value)
    {
        Value = value;
        // Calculate the length of the field by getting the number of bits needed to represent the value plus 1
        // then add 4 to round up to the next multiple of 5 and divide by 5 to get the number of bytes
        Length = (short)((BitOperations.Log2(Value) + 1 + 4) / 5);
    }

    /// <inheritdoc/>
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

    /// <summary>
    /// Reads a MinFinalCltvExpiryTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The MinFinalCltvExpiryTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static MinFinalCltvExpiryTaggedField FromBitReader(BitReader bitReader, short length)
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