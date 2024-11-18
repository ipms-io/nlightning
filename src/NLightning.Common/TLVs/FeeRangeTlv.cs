using System.Runtime.Serialization;

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

    public FeeRangeTlv(Tlv tlv) : base(TlvConstants.FEE_RANGE)
    {
        if (tlv.Type != TlvConstants.FEE_RANGE)
        {
            throw new Exception("Invalid TLV type");
        }

        if (tlv.Length != sizeof(ulong) * 2) // 2 long (128 bits) is 16 bytes
        {
            throw new Exception("Invalid length");
        }

        Length = tlv.Length;
        Value = tlv.Value;
        MinFeeSatoshis = EndianBitConverter.ToUInt64BigEndian(Value[..sizeof(ulong)]);
        MaxFeeSatoshis = EndianBitConverter.ToUInt64BigEndian(Value[sizeof(ulong)..]);
    }

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
    /// Deserialize a FeeRangeTlv from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized FeeRangeTlv.</returns>
    /// <exception cref="SerializationException">Error deserializing FeeRangeTlv</exception>
    public static new async Task<FeeRangeTlv> DeserializeAsync(Stream stream)
    {
        var tlv = await Tlv.DeserializeAsync(stream);

        return new FeeRangeTlv(tlv);
    }
}