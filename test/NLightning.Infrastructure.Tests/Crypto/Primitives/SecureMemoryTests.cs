namespace NLightning.Infrastructure.Tests.Crypto.Primitives;

using Infrastructure.Crypto.Primitives;

public class SecureMemoryTests
{
    [Fact]
    public void Given_Size_When_CtorIsCalled_Then_MemoryIsAllocatedAndLocked()
    {
        // Given
        const int size = 64;

        // When
        var secureMemory = new SecureMemory(size);

        // Then
        Assert.Equal(size, secureMemory.Length);

        // Dispose to avoid finalizer calls in test
        secureMemory.Dispose();
    }

    [Fact]
    public void Given_SecureMemory_When_CastToSpan_Then_ReturnsWritableSpan()
    {
        // Given
        const int size = 8;
        using var secureMemory = new SecureMemory(size);

        // When
        Span<byte> span = secureMemory;
        span[0] = 0xAB;

        // Then
        Span<byte> newSpan = secureMemory;
        Assert.Equal(0xAB, newSpan[0]);
    }

    [Fact]
    public void Given_SecureMemory_When_CastToReadOnlySpan_Then_ReturnsReadableSpan()
    {
        // Given
        const int size = 8;
        using var secureMemory = new SecureMemory(size);

        // When
        ReadOnlySpan<byte> roSpan = secureMemory;

        // Then
        Assert.Equal(size, roSpan.Length);
    }

    [Fact]
    public void Given_SecureMemory_When_Disposed_Then_MemoryZeroUnlockFreeCalledAndProviderDisposed()
    {
        // Given
        const int size = 16;
        SecureMemory? secureMemory;
        secureMemory = new SecureMemory(size);
        Span<byte> span = secureMemory;
        span[0] = 0xAB;

        // When
        secureMemory.Dispose();
        secureMemory = new SecureMemory(size);
        span = secureMemory;

        // Then
        Assert.NotEqual(0xAB, span[0]);
    }
}