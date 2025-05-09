using NBitcoin;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.ValueObjects;

namespace NLightning.Common.TLVs;

/// <summary>
/// Blinded Path TLV.
/// </summary>
/// <remarks>
/// The blinded path TLV is used in the UpdateAddHtlcMessage to communicate the blinded path key.
/// </remarks>
public class BlindedPathTlv : Tlv
{
    /// <summary>
    /// The blinded path key
    /// </summary>
    public PubKey PathKey { get; }

    public BlindedPathTlv(PubKey pathKey) : base(TlvConstants.BLINDED_PATH)
    {
        PathKey = pathKey;

        Value = PathKey.ToBytes();
        Length = Value.Length;
    }

    /// <summary>
    /// Cast BlindedPathTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast BlindedPathTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting BlindedPathTlv</exception>
    public static BlindedPathTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.BLINDED_PATH)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new BlindedPathTlv(new PubKey(tlv.Value));
    }
}