using System.Runtime.Serialization;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Serialization;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

/// <summary>
/// Funding Output Contribution TLV.
/// </summary>
/// <remarks>
/// The funding output contribution TLV is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
/// </remarks>
public class FundingOutputContributionTlv : Tlv
{
    private static IEndianConverter? s_endianConverter;
    private static IEndianConverter _endianConverter => 
        s_endianConverter ?? throw new InvalidOperationException("EndianConverter not initialized");
    
    public static void SetEndianConverter(IEndianConverter converter) => s_endianConverter = converter;
    
    /// <summary>
    /// The amount being contributed in satoshis
    /// </summary>
    public long Satoshis { get; }

    public FundingOutputContributionTlv(long satoshis) : base(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
    {
        Satoshis = satoshis;

        Value = _endianConverter.GetBytesBigEndian(Satoshis);
        Length = Value.Length;
    }

    /// <summary>
    /// Deserialize a NetworksTLV from a stream.
    /// </summary>
    /// <param name="tlv">The stream to deserialize from.</param>
    /// <returns>The deserialized NetworksTLV.</returns>
    /// <exception cref="SerializationException">Error deserializing NetworksTLV</exception>
    public static FundingOutputContributionTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.FUNDING_OUTPUT_CONTRIBUTION)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length != 8) // long (64 bits) is 8 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        return new FundingOutputContributionTlv(_endianConverter.ToInt64BigEndian(tlv.Value));
    }
}