namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Networks TLV.
/// </summary>
/// <remarks>
/// The networks TLV is used in the InitMessage to communicate the networks that the node supports.
/// </remarks>
public class NetworksTlv : Tlv
{
    /// <summary>
    /// The chain hashes.
    /// </summary>
    /// <remarks>
    /// The chain hashes are the hashes of the chains that the node supports.
    /// </remarks>
    public IEnumerable<ChainHash>? ChainHashes { get; private set; }

    public NetworksTlv(IEnumerable<ChainHash> chainHashes) : base(TlvConstants.NETWORKS)
    {
        ChainHashes = chainHashes.ToList();

        Value = new byte[ChainHash.LENGTH * ChainHashes.Count()];

        for (var i = 0; i < ChainHashes.Count(); i++)
        {
            byte[] chainHash = ChainHashes.ElementAt(i);
            chainHash.CopyTo(Value, i * ChainHash.LENGTH);
        }

        Length = Value.Length;
    }

    /// <summary>
    /// Cast a NetworksTLV from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast NetworksTLV.</returns>
    /// <exception cref="InvalidCastException">Error casting NetworksTLV</exception>
    public static NetworksTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.NETWORKS)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length % ChainHash.LENGTH != 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        var chainHashes = new List<ChainHash>();
        // split the Value into 32 bytes chinks and add it to the list
        for (var i = 0; i < tlv.Length; i += ChainHash.LENGTH)
        {
            chainHashes.Add(tlv.Value[i..(i + ChainHash.LENGTH)]);
        }

        return new NetworksTlv(chainHashes);
    }
}