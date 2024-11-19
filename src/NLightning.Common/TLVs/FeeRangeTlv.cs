namespace NLightning.Common.TLVs;

using BitUtils;
using Constants;
using Types;

/// <summary>
/// Fee Range TLV.
/// </summary>
/// <remarks>
/// The fee range TLV is used in the ClosingSignedMessage to set our accepted fee range.
/// </remarks>
public class FeeRangeTlv : Tlv
{
    /// <summary>
    /// The minimum acceptable fee in satoshis
    /// </summary>
    public ulong MinFeeSatoshis { get; private set; }

    /// <summary>
    /// The maximum acceptable fee in satoshis
    /// </summary>
    public ulong MaxFeeSatoshis { get; private set; }

    public FeeRangeTlv(ulong minFeeSatoshis, ulong maxFeeSatoshis) : base(TlvConstants.FEE_RANGE)
    {
        MinFeeSatoshis = minFeeSatoshis;
        MaxFeeSatoshis = maxFeeSatoshis;

        var minSatsBytes = EndianBitConverter.GetBytesBigEndian(MinFeeSatoshis);
        var maxSatsBytes = EndianBitConverter.GetBytesBigEndian(MaxFeeSatoshis);

        Length = sizeof(ulong) * 2;
        Value = new byte[Length];
        minSatsBytes.CopyTo(Value, 0);
        maxSatsBytes.CopyTo(Value, sizeof(ulong));
    }

    /// <summary>
    /// Cast FeeRangeTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast FeeRangeTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting FeeRangeTlv</exception>
    public static FeeRangeTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.FEE_RANGE)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length != sizeof(ulong) * 2) // 2 long (128 bits) is 16 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        var minFeeSatoshis = EndianBitConverter.ToUInt64BigEndian(tlv.Value[..sizeof(ulong)]);
        var maxFeeSatoshis = EndianBitConverter.ToUInt64BigEndian(tlv.Value[sizeof(ulong)..]);

        return new FeeRangeTlv(minFeeSatoshis, maxFeeSatoshis);
    }
}