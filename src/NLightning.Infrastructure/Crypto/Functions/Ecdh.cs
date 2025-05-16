using NLightning.Domain.Crypto.Constants;

namespace NLightning.Infrastructure.Crypto.Functions;

using Hashes;
using Interfaces;
using Primitives;

/// <summary>
/// The SecP256k1 DH function (
/// <see href="https://github.com/lightning/bolts/blob/master/08-transport.md#handshake-state">Bolt 8 - Handshake State</see>).
/// </summary>
internal sealed class Ecdh : IEcdh
{
    /// <inheritdoc/>
    /// <param name="k">Private Key</param>
    /// <param name="rk">Remote Static PubKey</param>
    /// <param name="sharedKey"></param>
    public void SecP256K1Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
    {
        NBitcoin.PubKey pubKey = new(rk);

        // ECDH operation
        var sharedPubKey = pubKey.GetSharedPubkey(k);

        // SHA256 hash of the compressed format of the shared public key
        using var sha256 = new Sha256();
        sha256.AppendData(sharedPubKey.Compress().ToBytes());
        sha256.GetHashAndReset(sharedKey);
    }

    /// <inheritdoc/>
    public KeyPair GenerateKeyPair()
    {
        return new KeyPair(new NBitcoin.Key());
    }

    /// <inheritdoc/>
    public KeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey)
    {
        if (privateKey.Length != CryptoConstants.PRIVKEY_LEN)
        {
            throw new ArgumentException("Invalid private key length");
        }

        return new KeyPair(new NBitcoin.Key(privateKey.ToArray()));
    }
}