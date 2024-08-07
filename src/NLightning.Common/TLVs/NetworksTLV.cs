using System.Runtime.Serialization;

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

    public NetworksTlv() : base(TlvConstants.NETWORKS)
    { }
    public NetworksTlv(IEnumerable<ChainHash> chainHashes) : base(TlvConstants.NETWORKS)
    {
        ChainHashes = chainHashes;
    }

    /// <summary>
    /// Serialize the TLV to a stream
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="SerializationException">Error serializing TLV or any of it's parts</exception>
    public new async Task SerializeAsync(Stream stream)
    {
        if (ChainHashes != null)
        {
            using var bufferStream = new MemoryStream();

            foreach (var chainHash in ChainHashes)
            {
                await bufferStream.WriteAsync(chainHash);
            }

            Length = bufferStream.Length;
            Value = bufferStream.ToArray();
        }

        await base.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserialize a NetworksTLV from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized NetworksTLV.</returns>
    /// <exception cref="SerializationException">Error deserializing NetworksTLV</exception>
    public static new async Task<NetworksTlv> DeserializeAsync(Stream stream)
    {
        var tlv = await Tlv.DeserializeAsync(stream) as NetworksTlv ?? throw new SerializationException("Invalid TLV type");

        if (tlv.Type != TlvConstants.NETWORKS)
        {
            throw new SerializationException("Invalid TLV type");
        }

        if (tlv.Length % ChainHash.LENGTH != 0)
        {
            throw new SerializationException("Invalid length");
        }

        var chainHashes = new List<ChainHash>();
        // split the Value into 32 bytes chinks and add it to the list
        for (var i = 0; i < tlv.Length; i += ChainHash.LENGTH)
        {
            chainHashes.Add(tlv.Value[i..(i + ChainHash.LENGTH)]);
        }
        tlv.ChainHashes = chainHashes;

        return tlv;
    }
}