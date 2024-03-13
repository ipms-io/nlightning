namespace NLightning.Bolts.BOLT8.Noise.Constants;

using Hashes;
using Interfaces;
using Primitives;

/// <summary>
/// The Secp256k1 DH function (
/// <see href="https://github.com/lightning/bolts/blob/master/08-transport.md">Bolt 8</see>).
/// </summary>
internal sealed class Secp256k1 : IDh
{
    // private static readonly X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
    // private static readonly ECDomainParameters domain = new(curve.Curve, curve.G, curve.N, curve.H);

    public int PrivLen => 32; // Size of PrivateKey for Bitcoin
    public int PubLen => 33; // Size of PubKey for Bitcoin

    /// <summary>
    /// Performs an Elliptic-Curve Diffie-Hellman operation.
    /// </summary>
    /// <param name="k">Key Pair containing a Private Key</param>
    /// <param name="rk">Remote Static PubKey</param>
    /// <param name="sharedKey"></param>
    public void Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
    {
        NBitcoin.PubKey pubKey = new(rk);

        // ECDH operation
        NBitcoin.PubKey sharedPubKey = pubKey.GetSharedPubkey(k);

        // SHA256 hash of the compressed format of the shared public key
        using var hasher = new SHA256();
        hasher.AppendData(sharedPubKey.Compress().ToBytes());
        hasher.GetHashAndReset(sharedKey);
    }

    public KeyPair GenerateKeyPair()
    {
        return new KeyPair(new NBitcoin.Key());
    }

    public KeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey)
    {
        return new KeyPair(new NBitcoin.Key(privateKey.ToArray()));
    }
}