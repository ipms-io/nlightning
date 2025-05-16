using NBitcoin;

namespace NLightning.Domain.Protocol.Tlv;

using Constants;
using Models;

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
    public PubKey PathKey { get; internal set; }

    public BlindedPathTlv(PubKey pathKey) : base(TlvConstants.BLINDED_PATH)
    {
        PathKey = pathKey;

        Value = PathKey.ToBytes();
        Length = Value.Length;
    }
}