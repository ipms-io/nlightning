namespace NLightning.Common.Crypto.Hashes;

using Constants;
using Factories.Crypto;
using Interfaces.Crypto;

public sealed class Argon2Id : IDisposable
{
    private const ulong DERIVE_KEY_MEM_LIMIT = 1 << 16; // 64 MiB
    private const ulong DERIVE_KEY_OPS_LIMIT = 3;

    private readonly ICryptoProvider _cryptoProvider;

    public Argon2Id()
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
    }

    public void DeriveKeyFromPasswordAndSalt(string password, ReadOnlySpan<byte> salt, Span<byte> key)
    {
        if (key.Length != CryptoConstants.PRIVKEY_LEN)
            throw new ArgumentException($"Key must be {CryptoConstants.PRIVKEY_LEN} bytes long", nameof(key));

        var ret = _cryptoProvider
            .DeriveKeyFromPasswordUsingArgon2I(key, password, salt, DERIVE_KEY_OPS_LIMIT, DERIVE_KEY_MEM_LIMIT);

        if (ret != 0)
            throw new Exception("Argon2ID key derivation failed");
    }

    public void Dispose()
    {
        _cryptoProvider.Dispose();
    }
}