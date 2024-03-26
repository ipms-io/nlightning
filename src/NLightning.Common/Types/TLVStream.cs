using System.Runtime.Serialization;

namespace NLightning.Common.Types;

/// <summary>
/// A series of (possibly zero) TLVs
/// </summary>
public sealed class TLVStream
{
    private HashSet<TLV> _tlvs = [];

    public TLVStream()
    { }
    public TLVStream(BinaryReader reader)
    {
        Deserialize(reader);
    }

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
    /// Get a specific TLV from the stream
    /// </summary>
    /// <param name="type">The type of TLV to get</param>
    /// <returns></returns>
    public bool TryGetTlv(BigSize type, out TLV? tlv)
    {
        tlv = _tlvs.FirstOrDefault(tlv => tlv.Type == type);
        return tlv != null;
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
    public void Serialize(BinaryWriter writer)
    {
        foreach (var tlv in _tlvs)
        {
            writer.Write(tlv.Serialize());
        }
    }

    /// <summary>
    /// Deserialize a TLV stream
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <returns>The TLV stream</returns>
    /// <exception cref="SerializationException">Error deserializing TLVStream or any of it's TLVs</exception>
    public void Deserialize(BinaryReader reader)
    {
        try
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                Add(TLV.Deserialize(reader));
            }
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TLVStream", e);
        }
    }
}