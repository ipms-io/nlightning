namespace NLightning.Common.Interfaces.Crypto;

internal interface ICryptoProvider : IDisposable
{
    #region Sha256
    void Sha256Init(IntPtr state);

    void Sha256Update(IntPtr state, ReadOnlySpan<byte> data);

    void Sha256Final(IntPtr state, Span<byte> result);
    #endregion

    #region AeadChacha20Poly1305Ietf
    int AeadChacha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                         ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                         ReadOnlySpan<byte> plainText, Span<byte> cipherText, out long cipherTextLength);
    int AeadChacha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
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
}