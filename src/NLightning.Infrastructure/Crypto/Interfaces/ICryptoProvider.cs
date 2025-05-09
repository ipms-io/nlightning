namespace NLightning.Infrastructure.Crypto.Interfaces;

internal interface ICryptoProvider : IDisposable
{
    #region Sha256
    void Sha256Init(IntPtr state);

    void Sha256Update(IntPtr state, ReadOnlySpan<byte> data);

    void Sha256Final(IntPtr state, Span<byte> result);
    #endregion

    #region AeadChacha20Poly1305Ietf
    int AeadChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                         ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                         ReadOnlySpan<byte> plainText, Span<byte> cipherText, out long cipherTextLength);
    int AeadChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                         ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                         ReadOnlySpan<byte> cipherText, Span<byte> plainText, out long plainTextLength);
    #endregion

    #region Memory Operations
    IntPtr MemoryAlloc(ulong size);
    int MemoryLock(IntPtr addr, ulong len);
    void MemoryFree(IntPtr ptr);
    void MemoryZero(IntPtr ptr, ulong len);
    void MemoryUnlock(IntPtr addr, ulong len);
    #endregion

    #region AeadXChaCha20Poly1305Ietf
    int AeadXChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
                                         ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> plainText,
                                         Span<byte> cipherText, out long cipherTextLength);

    int AeadXChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
                                         ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> cipherText,
                                         Span<byte> plainText, out long plainTextLength);
    #endregion

    #region Key Derivation From Password
    int DeriveKeyFromPasswordUsingArgon2I(Span<byte> key, string password, ReadOnlySpan<byte> salt, ulong opsLimit,
                                          ulong memLimit);
    #endregion

    #region Random
    void RandomBytes(Span<byte> buffer);
    #endregion
}