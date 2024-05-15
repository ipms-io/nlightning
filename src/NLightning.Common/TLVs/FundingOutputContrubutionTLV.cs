using System.Runtime.Serialization;

namespace NLightning.Common.TLVs;

using Constants;
using Types;
using Utils;

/// <summary>
/// Funding Output Contrubution TLV.
/// </summary>
/// <remarks>
/// The funding output contribution TLV is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
/// </remarks>
public class FundingOutputContrubutionTLV : TLV
{
    /// <summary>
    /// The amount being contributed in satoshis
    /// </summary>
    public long Satoshis { get; private set; }

    public FundingOutputContrubutionTLV() : base(TLVConstants.FUNDING_OUTPUT_CONTRIBUTION)
    { }
    public FundingOutputContrubutionTLV(long satoshis) : base(TLVConstants.FUNDING_OUTPUT_CONTRIBUTION)
    {
        Satoshis = satoshis;
    }

    /// <inheritdoc/>
    public new async Task SerializeAsync(Stream stream)
    {
        var satoshiBytes = EndianBitConverter.GetBytesBE(Satoshis);

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
    public static new async Task<FundingOutputContrubutionTLV> DeserializeAsync(Stream stream)
    {
        var tlv = await TLV.DeserializeAsync(stream) as FundingOutputContrubutionTLV ?? throw new SerializationException("Invalid TLV type");

        if (tlv.Type != TLVConstants.FUNDING_OUTPUT_CONTRIBUTION)
        {
            throw new SerializationException("Invalid TLV type");
        }

        if (tlv.Length != 8) // long (64 bits) is 8 bytes
        {
            throw new SerializationException("Invalid length");
        }

        tlv.Satoshis = EndianBitConverter.ToInt64BE(tlv.Value);

        return tlv;
    }
}