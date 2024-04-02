using System.Runtime.Serialization;

namespace NLightning.Common.Types;

/// <summary>
/// A series of (possibly zero) TLVs
/// </summary>
public sealed class TLVStream
{
    private readonly SortedDictionary<BigSize, TLV> _tlvs = [];

    /// <summary>
    /// Add a TLV to the stream
    /// </summary>
    /// <param name="tlv">The TLV to add</param>
    public void Add(TLV tlv)
    {
        if (_tlvs.ContainsKey(tlv.Type))
        {
            throw new ArgumentException($"A TLV with type {tlv.Type} already exists.");
        }

        _tlvs[tlv.Type] = tlv;
    }

    /// <summary>
    /// Get all TLVs in the stream
    /// </summary>
    public IEnumerable<TLV> GetTlvs()
    {
        return _tlvs.Values;
    }

    /// <summary>
    /// Get a specific TLV from the stream
    /// </summary>
    /// <param name="type">The type of TLV to get</param>
    /// <returns></returns>
    public bool TryGetTlv(BigSize type, out TLV? tlv)
    {
        return _tlvs.TryGetValue(type, out tlv);
    }

    /// <summary>
    /// Check if any TLVs are present
    /// </summary>
    public bool Any()
    {
        return _tlvs.Count != 0;
    }

    /// <summary>
    /// Serialize the TLV stream
    /// </summary>
    public async Task SerializeAsync(Stream stream)
    {
        foreach (var tlv in _tlvs)
        {
            await tlv.Value.SerializeAsync(stream);
        }
    }

    /// <summary>
    /// Deserialize a TLV stream
    /// </summary>
    /// <param name="reader">The reader to use</param>
    /// <returns>The TLV stream</returns>
    /// <exception cref="SerializationException">Error deserializing TLVStream or any of it's TLVs</exception>
    public static async Task<TLVStream?> DeserializeAsync(Stream stream)
    {
        if (stream.Position == stream.Length)
        {
            return null;
        }

        try
        {
            var tlvStream = new TLVStream();

            while (stream.Position != stream.Length)
            {
                tlvStream.Add(await TLV.DeserializeAsync(stream));
            }

            return tlvStream;
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TLVStream", e);
        }
    }
}