namespace NLightning.Domain.Protocol.Tlv;

using Constants;
using Domain.Crypto.Constants;
using ValueObjects;

/// <summary>
/// Networks TLV.
/// </summary>
/// <remarks>
/// The networks TLV is used in the InitMessage to communicate the networks that the node supports.
/// </remarks>
public class NetworksTlv : BaseTlv
{
    /// <summary>
    /// The chain hashes.
    /// </summary>
    /// <remarks>
    /// The chain hashes are the hashes of the chains that the node supports.
    /// </remarks>
    public IEnumerable<ChainHash>? ChainHashes { get; }

    public NetworksTlv(IEnumerable<ChainHash> chainHashes) : base(TlvConstants.Networks)
    {
        ChainHashes = chainHashes.ToList();

        Value = new byte[CryptoConstants.Sha256HashLen * ChainHashes.Count()];

        for (var i = 0; i < ChainHashes.Count(); i++)
        {
            byte[] chainHash = ChainHashes.ElementAt(i);
            chainHash.CopyTo(Value, i * CryptoConstants.Sha256HashLen);
        }

        Length = Value.Length;
    }
}