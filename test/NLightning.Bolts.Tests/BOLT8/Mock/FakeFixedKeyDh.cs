namespace NLightning.Bolts.Tests.BOLT8.Mock;

using Bolts.BOLT8.Dhs;
using Bolts.BOLT8.Interfaces;
using Bolts.BOLT8.Primitives;

internal class FakeFixedKeyDh(byte[] privateKey) : IDh
{
    private readonly SecP256K1 _dh = new();

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