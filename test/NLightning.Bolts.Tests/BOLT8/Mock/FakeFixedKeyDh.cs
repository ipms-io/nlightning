using NLightning.Bolts.BOLT8.Dhs;
using NLightning.Bolts.BOLT8.Interfaces;
using NLightning.Bolts.BOLT8.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Mock;

internal class FakeFixedKeyDh(byte[] privateKey) : IDh
{
    private readonly Secp256k1 _dh = new();

    public KeyPair GenerateKeyPair()
    {
        return _dh.GenerateKeyPair(privateKey);
    }

    public KeyPair GenerateKeyPair(ReadOnlySpan<byte> privKey)
    {
        return _dh.GenerateKeyPair(privKey);
    }

    public void Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
    {
        _dh.Dh(k, rk, sharedKey);
    }
}