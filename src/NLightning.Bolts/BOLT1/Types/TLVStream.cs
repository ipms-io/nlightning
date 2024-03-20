using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT1.Types;

/// <summary>
/// A series of (possibly zero) TLVs
/// </summary>
public sealed class TLVStream()
{
    private HashSet<TLV> _tlvs = [];

    /// <summary>
    /// Add a TLV to the stream
    /// </summary>
    /// <param name="tlv">The TLV to add</param>
    public void Add(TLV tlv)
    {
        _tlvs.Add(tlv);
    }

    /// <summary>
    /// Get all TLVs in the stream
    /// </summary>
    public IEnumerable<TLV> GetTlvs()
    {
        return _tlvs;
    }

    /// <summary>
    /// Check if any TLVs are present
    /// </summary>
    public bool Any()
    {
        return _tlvs.Count != 0;
    }

    /// <summary>
    /// Order the TLVs by type
    /// </summary>
    public IEnumerable<TLV> Ordered()
    {
        _tlvs = [.. _tlvs.OrderBy(tlv => tlv.Type)];
        return _tlvs;
    }

    /// <summary>
    /// Serialize the TLV stream
    /// </summary>
    public byte[] Serialize()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        foreach (var tlv in _tlvs)
        {
            writer.Write(tlv.Serialize());
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Deserialize a TLV stream
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <returns>The TLV stream</returns>
    /// <exception cref="SerializationException">Error deserializing TLVStream or any of it's TLVs</exception>
    public static TLVStream? Deserialize(BinaryReader reader)
    {
        if (reader.BaseStream.Position == reader.BaseStream.Length)
        {
            return null;
        }

        try
        {
            var tlvStream = new TLVStream();

            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                tlvStream.Add(TLV.Deserialize(reader));
            }

            return tlvStream;
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TLVStream", e);
        }
    }
}