using NBitcoin;

namespace NLightning.Bolt11.Models.TaggedFields;

using Constants;
using Domain.Utils;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the payment hash
/// </summary>
/// <remarks>
/// The payment hash is a 32-byte hash used to identify a payment
/// </remarks>
/// <seealso cref="ITaggedField"/>
internal sealed class PaymentHashTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.PaymentHash;
    internal uint256 Value { get; }
    public short Length => TaggedFieldConstants.HashLength;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentHashTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description Hash</param>
    internal PaymentHashTaggedField(uint256 value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        var data = Value.ToBytes();
        if (BitConverter.IsLittleEndian)
            Array.Reverse(data);

        // Write data
        bitWriter.WriteBits(data, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return Value != uint256.Zero && Value != uint256.One;
    }

    /// <summary>
    /// Reads a PaymentHashTaggedField from a BitReader
    /// </summary>
    /// <param name="bitReader">The BitReader to read from</param>
    /// <param name="length">The length of the field</param>
    /// <returns>The PaymentHashTaggedField</returns>
    /// <exception cref="ArgumentException">Thrown when the length is invalid</exception>
    internal static PaymentHashTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length != TaggedFieldConstants.HashLength)
            throw new ArgumentException(
                $"Invalid length for {nameof(PaymentHashTaggedField)}. Expected {TaggedFieldConstants.HashLength}, but got {length}");

        // Read the data from the BitReader
        var data = new byte[(TaggedFieldConstants.HashLength * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);
        data = data[..^1];

        if (BitConverter.IsLittleEndian)
            Array.Reverse(data);

        return new PaymentHashTaggedField(new uint256(data));
    }
}