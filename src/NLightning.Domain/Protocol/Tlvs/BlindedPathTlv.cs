using NBitcoin;
using NLightning.Domain.Protocol.Models;

namespace NLightning.Domain.Protocol.Tlvs;

using Constants;

/// <summary>
/// Blinded Path TLV.
/// </summary>
/// <remarks>
/// The blinded path TLV is used in the UpdateAddHtlcMessage to communicate the blinded path key.
/// </remarks>
public class BlindedPathTlv : BaseTlv
{
    /// <summary>
    /// The blinded path key
    /// </summary>
    public PubKey PathKey { get; }

    public BlindedPathTlv(PubKey pathKey) : base(TlvConstants.BlindedPath)
    {
        PathKey = pathKey;

        Value = PathKey.ToBytes();
        Length = Value.Length;
    }

    /// <summary>
    /// Cast BlindedPathTlv from a BaseTlv.
    /// </summary>
    /// <param name="baseTlv">The baseTlv to cast from.</param>
    /// <returns>The cast BlindedPathTlv.</returns>
    /// <exception cref="InvalidCastException">Error casting BlindedPathTlv</exception>
    public static BlindedPathTlv FromTlv(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.BlindedPath)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new BlindedPathTlv(new PubKey(baseTlv.Value));
    }
}