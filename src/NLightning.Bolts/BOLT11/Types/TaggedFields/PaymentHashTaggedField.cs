using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Constants;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for the payment hash
/// </summary>
/// <remarks>
/// The payment hash is a 32 byte hash that is used to identify a payment
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class PaymentHashTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.PAYMENT_HASH;
    public uint256 Value { get; }
    public short Length => TaggedFieldConstants.HASH_LENGTH;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentHashTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description Hash</param>
    public PaymentHashTaggedField(uint256 value)
    {
        Value = value;
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        var data = Value.ToBytes();
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data);
        }

        // Write data
        bitWriter.WriteBits(data, Length * 5);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        return Value != uint256.Zero;
    }

    public object GetValue()
    {
        return Value;
    }

    public static PaymentHashTaggedField FromBitReader(BitReader bitReader, short length)
    {
        if (length != TaggedFieldConstants.HASH_LENGTH)
        {
            throw new ArgumentException($"Invalid length for PaymentHashTaggedField. Expected {TaggedFieldConstants.HASH_LENGTH}, but got {length}");
        }

        // Read the data from the BitReader
        var data = new byte[(TaggedFieldConstants.HASH_LENGTH * 5 + 7) / 8];
        bitReader.ReadBits(data, length * 5);
        data = data[..^1];

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(data);
        }

        return new PaymentHashTaggedField(new uint256(data));
    }
}