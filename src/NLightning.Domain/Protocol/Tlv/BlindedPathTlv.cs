using NLightning.Domain.Crypto;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Protocol.Tlv;

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
    public CompactPubKey PathKey { get; }

    public BlindedPathTlv(CompactPubKey pathKey) : base(TlvConstants.BlindedPath)
    {
        PathKey = pathKey;

        Value = PathKey.CompactBytes;
        Length = Value.Length;
    }
}