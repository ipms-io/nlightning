namespace NLightning.Infrastructure.Crypto.Primitives;

using Factories;
using Interfaces;

public sealed class SecureMemory : IDisposable
{
    private readonly ICryptoProvider _cryptoProvider;
    private readonly IntPtr _handle;

    public int Length { get; }

    public SecureMemory(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be positive.");

        _cryptoProvider = CryptoFactory.GetCryptoProvider();
        Length = size;
        _handle = _cryptoProvider.MemoryAlloc((ulong)size);

        if (_handle == IntPtr.Zero)
            throw new OutOfMemoryException("Failed to allocate secure memory.");

        try
        {
            _cryptoProvider.MemoryLock(new IntPtr(_handle), (ulong)Length);
        }
        catch
        {
            _cryptoProvider.MemoryFree(new IntPtr(_handle));
            throw;
        }
    }

    #region Implicit Conversions

    public static unsafe implicit operator Span<byte>(SecureMemory secureMemory)
    {
        ArgumentNullException.ThrowIfNull(secureMemory);
        return secureMemory._disposed
            ? throw new ObjectDisposedException(nameof(SecureMemory))
            : new Span<byte>(secureMemory._handle.ToPointer(), secureMemory.Length);
    }

    public static unsafe implicit operator ReadOnlySpan<byte>(SecureMemory secureMemory)
    {
        ArgumentNullException.ThrowIfNull(secureMemory);
        return secureMemory._disposed
            ? throw new ObjectDisposedException(nameof(SecureMemory))
            : new ReadOnlySpan<byte>(secureMemory._handle.ToPointer(), secureMemory.Length);
    }
    #endregion

    public override bool Equals(object? obj)
    {
        if (obj is not SecureMemory castObj) return false;

        return castObj.Length == Length && castObj._handle == _handle;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Length, _handle);
    }

    #region Dispose Pattern
    private bool _disposed;
    private void ReleaseUnmanagedResources()
    {
        if (_handle == IntPtr.Zero)
            return;

        try
        {
            _cryptoProvider.MemoryZero(_handle, (ulong)Length);
        }
        finally
        {
            try
            {
                _cryptoProvider.MemoryUnlock(_handle, (ulong)Length);
            }
            finally
            {
                _cryptoProvider.MemoryFree(_handle);
            }
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        ReleaseUnmanagedResources();
        if (disposing)
        {
            _cryptoProvider.Dispose();
        }

        _disposed = true;
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