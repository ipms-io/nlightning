using System.Diagnostics.CodeAnalysis;
using NLightning.Infrastructure.Bitcoin.Crypto.Functions;

namespace NLightning.Tests.Utils.Mocks;

using Domain.Crypto.ValueObjects;
using Infrastructure.Crypto.Interfaces;

[ExcludeFromCodeCoverage]
internal class FakeFixedKeyDh(byte[] privateKey) : IEcdh
{
    private readonly Ecdh _ecdh = new();

    public CryptoKeyPair GenerateKeyPair()
    {
        return _ecdh.GenerateKeyPair(privateKey);
    }

    public CryptoKeyPair GenerateKeyPair(ReadOnlySpan<byte> privKey)
    {
        return _ecdh.GenerateKeyPair(privKey);
    }

    public void SecP256K1Dh(PrivKey k, ReadOnlySpan<byte> rk, Span<byte> sharedKey)
    {
        _ecdh.SecP256K1Dh(k, rk, sharedKey);
    }
}