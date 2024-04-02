using System.Runtime.Serialization;

namespace NLightning.Common.TLVs;

using Constants;
using Types;

public class NetworksTLV : TLV
{
    public IEnumerable<ChainHash>? ChainHashes { get; private set; }

    public NetworksTLV() : base(TLVConstants.NETWORKS)
    { }
    public NetworksTLV(IEnumerable<ChainHash> chainHashes) : base(TLVConstants.NETWORKS)
    {
        ChainHashes = chainHashes;
    }

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

    public static new async Task<NetworksTLV> DeserializeAsync(Stream stream)
    {
        var tlv = await TLV.DeserializeAsync(stream) as NetworksTLV ?? throw new SerializationException("Invalid TLV type");

        if (tlv.Type != TLVConstants.NETWORKS)
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