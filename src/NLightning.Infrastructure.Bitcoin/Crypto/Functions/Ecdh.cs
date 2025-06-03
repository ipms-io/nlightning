using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Crypto.Functions;

using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Infrastructure.Crypto.Hashes;
using Infrastructure.Crypto.Interfaces;

/// <summary>
/// The SecP256k1 DH function implementation.
/// </summary>
/// <see href="https://github.com/lightning/bolts/blob/master/08-transport.md#handshake-state"/>
internal sealed class Ecdh : IEcdh
{
    /// <inheritdoc/>
    /// <param name="k">Private Key</param>
    /// <param name="rk">Remote Static PubKey</param>
    /// <param name="sharedKey"></param>
    public void SecP256K1Dh(PrivKey k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
    {
        PubKey pubKey = new(rk);
        using Key key = new(k);

        // ECDH operation
        var sharedPubKey = pubKey.GetSharedPubkey(key);

        // Shared public key's Compressed format's SHA256
        using var sha256 = new Sha256();
        sha256.AppendData(sharedPubKey.Compress().ToBytes());
        sha256.GetHashAndReset(sharedKey);
    }

    /// <inheritdoc/>
    public CryptoKeyPair GenerateKeyPair()
    {
        using var key = new Key();
        return new CryptoKeyPair(key.ToBytes(), key.PubKey.ToBytes());
    }

    /// <inheritdoc/>
    public CryptoKeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey)
    {
        if (privateKey.Length != CryptoConstants.PrivkeyLen)
        {
            throw new ArgumentException("Invalid private key length");
        }

        using var key = new Key(privateKey.ToArray());
        return new CryptoKeyPair(key.ToBytes(), key.PubKey.ToBytes());
    }
}