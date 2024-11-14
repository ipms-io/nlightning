using System.Runtime.Serialization;

namespace NLightning.Common.TLVs;

using BitUtils;
using Constants;
using Types;

/// <summary>
/// Funding Output Contribution TLV.
/// </summary>
/// <remarks>
/// The funding output contribution TLV is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
/// </remarks>
public class FundingOutputContributionTlv : Tlv
{
    /// <summary>
    /// The amount being contributed in satoshis
    /// </summary>
    public long Satoshis { get; private set; }

    public FundingOutputContributionTlv() : base(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
    { }
    public FundingOutputContributionTlv(long satoshis) : base(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
    {
        Satoshis = satoshis;
    }

    /// <summary>
    /// Serialize the TLV to a stream
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="SerializationException">Error serializing TLV or any of it's parts</exception>
    public new async Task SerializeAsync(Stream stream)
    {
        var satoshiBytes = EndianBitConverter.GetBytesBigEndian(Satoshis);

        Length = satoshiBytes.Length;
        Value = satoshiBytes;

        await base.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserialize a NetworksTLV from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized NetworksTLV.</returns>
    /// <exception cref="SerializationException">Error deserializing NetworksTLV</exception>
    public static new async Task<FundingOutputContributionTlv> DeserializeAsync(Stream stream)
    {
        var tlv = await Tlv.DeserializeAsync(stream) as FundingOutputContributionTlv ?? throw new SerializationException("Invalid TLV type");

        if (tlv.Type != TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
        {
            throw new SerializationException("Invalid TLV type");
        }

        if (tlv.Length != 8) // long (64 bits) is 8 bytes
        {
            throw new SerializationException("Invalid length");
        }

        tlv.Satoshis = EndianBitConverter.ToInt64BigEndian(tlv.Value);

        return tlv;
    }
}