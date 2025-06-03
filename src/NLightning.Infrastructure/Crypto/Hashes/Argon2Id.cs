namespace NLightning.Infrastructure.Crypto.Hashes;

using Domain.Crypto.Constants;
using Factories;
using Interfaces;

public sealed class Argon2Id : IDisposable
{
    private const ulong DeriveKeyMemLimit = 1 << 16; // 64 MiB
    private const ulong DeriveKeyOpsLimit = 3;

    private readonly ICryptoProvider _cryptoProvider;

    public Argon2Id()
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
    }

    public void DeriveKeyFromPasswordAndSalt(string password, ReadOnlySpan<byte> salt, Span<byte> key)
    {
        if (key.Length != CryptoConstants.PrivkeyLen)
            throw new ArgumentException($"Key must be {CryptoConstants.PrivkeyLen} bytes long", nameof(key));

        var ret = _cryptoProvider
            .DeriveKeyFromPasswordUsingArgon2I(key, password, salt, DeriveKeyOpsLimit, DeriveKeyMemLimit);

        if (ret != 0)
            throw new Exception("Argon2ID key derivation failed");
    }

    public void Dispose()
    {
        _cryptoProvider.Dispose();
    }
}