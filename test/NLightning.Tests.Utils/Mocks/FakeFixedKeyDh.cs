using System.Diagnostics.CodeAnalysis;

namespace NLightning.Tests.Utils.Mocks;

using Infrastructure.Crypto.Functions;
using Infrastructure.Crypto.Interfaces;
using Infrastructure.Crypto.Primitives;

[ExcludeFromCodeCoverage]
internal class FakeFixedKeyDh(byte[] privateKey) : IEcdh
{
    private readonly Ecdh _ecdh = new();

    public KeyPair GenerateKeyPair()
    {
        return _ecdh.GenerateKeyPair(privateKey);
    }

    public KeyPair GenerateKeyPair(ReadOnlySpan<byte> privKey)
    {
        return _ecdh.GenerateKeyPair(privKey);
    }

    public void SecP256K1Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
    {
        _ecdh.SecP256K1Dh(k, rk, sharedKey);
    }
}