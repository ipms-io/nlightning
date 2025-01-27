namespace NLightning.Common.Crypto.Primitives;

using Factories.Crypto;
using Interfaces.Crypto;

public sealed unsafe class SecureMemory : IDisposable
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly void* _pointer;

    public int Length { get; }

    public SecureMemory(int size)
    {
        _cryptoProvider = CryptoFactory.GetCryptoProvider();
        Length = size;
        _pointer = _cryptoProvider.MemoryAlloc((ulong)size).ToPointer();

        _cryptoProvider.MemoryLock(new IntPtr(_pointer), (ulong)Length);
    }

    #region Implicit Conversions
    public static implicit operator Span<byte>(SecureMemory secureMemory) => new(secureMemory._pointer, secureMemory.Length);
    public static implicit operator ReadOnlySpan<byte>(SecureMemory secureMemory) => new(secureMemory._pointer, secureMemory.Length);
    #endregion

    public override bool Equals(object? obj)
    {
        if (obj is not SecureMemory castObj) return false;

        return castObj.Length == Length && castObj._pointer == _pointer;
    }
    public override int GetHashCode()
    {
        return Length ^ (int)new IntPtr(_pointer);
    }

    #region Dispose Pattern
    private void ReleaseUnmanagedResources()
    {
        var pointerInt = new IntPtr(_pointer);
        _cryptoProvider.MemoryZero(pointerInt, (ulong)Length);
        _cryptoProvider.MemoryUnlock(pointerInt, (ulong)Length);
        _cryptoProvider.MemoryFree(pointerInt);
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _cryptoProvider.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecureMemory()
    {
        Dispose(false);
    }
    #endregion
}