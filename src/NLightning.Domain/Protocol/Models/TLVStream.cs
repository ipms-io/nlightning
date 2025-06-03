using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Models;

using Tlv;

/// <summary>
/// A series of (possibly zero) TLVs
/// </summary>
public sealed class TlvStream
{
    private readonly SortedDictionary<BigSize, BaseTlv> _tlvs = [];

    /// <summary>
    /// Add a TLV to the stream
    /// </summary>
    /// <param name="baseTlv">The TLV to add</param>
    public void Add(BaseTlv baseTlv)
    {
        if (!_tlvs.TryAdd(baseTlv.Type, baseTlv))
        {
            throw new ArgumentException($"A TLV with type {baseTlv.Type} already exists.");
        }
    }

    /// <summary>
    /// Add a series of TLV to the stream
    /// </summary>
    /// <param name="tlvs">The TLVs to add</param>
    public void Add(params BaseTlv?[] tlvs)
    {
        foreach (var tlv in tlvs)
        {
            if (tlv is null)
                continue;

            if (!_tlvs.TryAdd(tlv.Type, tlv))
            {
                throw new ArgumentException($"A TLV with type {tlv.Type} already exists.");
            }
        }
    }

    /// <summary>
    /// Get all TLVs in the stream
    /// </summary>
    public IEnumerable<BaseTlv> GetTlvs()
    {
        return _tlvs.Values;
    }

    /// <summary>
    /// Get a specific TLV from the stream
    /// </summary>
    /// <param name="type">The type of TLV to get</param>
    /// <param name="tlv">The TLV to get</param>
    /// <returns></returns>
    public bool TryGetTlv(BigSize type, out BaseTlv? tlv)
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
}