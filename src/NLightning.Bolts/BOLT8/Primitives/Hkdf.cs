using System.Diagnostics;

namespace NLightning.Bolts.BOLT8.Primitives;

using Hashes;
using Constants;

/// <summary>
/// HMAC-based Extract-and-Expand Key Derivation Function, defined in
/// <see href="https://tools.ietf.org/html/rfc5869">RFC 5869</see>.
/// </summary>
internal sealed class Hkdf : IDisposable
{
    private static readonly byte[] s_one = [1];
    private static readonly byte[] s_two = [2];
    private static readonly byte[] s_three = [3];

    private readonly SHA256 _hasher = new();
    private bool _disposed;

    /// <summary>
    /// Takes a chainingKey byte sequence of length HashLen,
    /// and an inputKeyMaterial byte sequence with length
    /// either zero bytes, 32 bytes, or DhLen bytes. Writes a
    /// byte sequences of length 2 * HashLen into output parameter.
    /// </summary>
    public void ExtractAndExpand2(ReadOnlySpan<byte> chainingKey, ReadOnlySpan<byte> inputKeyMaterial, Span<byte> output)
    {
        var hashLen = HashConstants.HASH_LEN;

        Debug.Assert(chainingKey.Length == hashLen);
        Debug.Assert(output.Length == 2 * hashLen);

        Span<byte> tempKey = stackalloc byte[hashLen];
        HmacHash(chainingKey, tempKey, inputKeyMaterial);

        var output1 = output[..hashLen];
        HmacHash(tempKey, output1, s_one);

        var output2 = output.Slice(hashLen, hashLen);
        HmacHash(tempKey, output2, output1, s_two);
    }

    /// <summary>
    /// Takes a chainingKey byte sequence of length HashLen,
    /// and an inputKeyMaterial byte sequence with length
    /// either zero bytes, 32 bytes, or DhLen bytes. Writes a
    /// byte sequences of length 3 * HashLen into output parameter.
    /// </summary>
    public void ExtractAndExpand3(ReadOnlySpan<byte> chainingKey, ReadOnlySpan<byte> inputKeyMaterial, Span<byte> output)
    {
        var hashLen = HashConstants.HASH_LEN;

        Debug.Assert(chainingKey.Length == hashLen);
        Debug.Assert(output.Length == 3 * hashLen);

        Span<byte> tempKey = stackalloc byte[hashLen];
        HmacHash(chainingKey, tempKey, inputKeyMaterial);

        var output1 = output[..hashLen];
        HmacHash(tempKey, output1, s_one);

        var output2 = output.Slice(hashLen, hashLen);
        HmacHash(tempKey, output2, output1, s_two);

        var output3 = output.Slice(2 * hashLen, hashLen);
        HmacHash(tempKey, output3, output2, s_three);
    }

    private void HmacHash(ReadOnlySpan<byte> key, Span<byte> hmac, ReadOnlySpan<byte> data1 = default, ReadOnlySpan<byte> data2 = default)
    {
        Debug.Assert(key.Length == HashConstants.HASH_LEN);
        Debug.Assert(hmac.Length == HashConstants.HASH_LEN);

        var blockLen = HashConstants.BLOCK_LEN;

        Span<byte> ipad = stackalloc byte[blockLen];
        Span<byte> opad = stackalloc byte[blockLen];

        key.CopyTo(ipad);
        key.CopyTo(opad);

        for (var i = 0; i < blockLen; ++i)
        {
            ipad[i] ^= 0x36;
            opad[i] ^= 0x5C;
        }

        _hasher.AppendData(ipad);
        _hasher.AppendData(data1);
        _hasher.AppendData(data2);
        _hasher.GetHashAndReset(hmac);

        _hasher.AppendData(opad);
        _hasher.AppendData(hmac);
        _hasher.GetHashAndReset(hmac);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _hasher.Dispose();
            _hasher.Dispose();
            _disposed = true;
        }
    }
}