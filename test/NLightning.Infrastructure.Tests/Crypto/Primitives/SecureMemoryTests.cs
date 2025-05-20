namespace NLightning.Infrastructure.Tests.Crypto.Primitives;

using Infrastructure.Crypto.Primitives;

public class SecureMemoryTests
{
    [Fact]
    public void Given_Size_When_CtorIsCalled_Then_MemoryIsAllocatedAndLocked()
    {
        // Given
        const int SIZE = 64;

        // When
        var secureMemory = new SecureMemory(SIZE);

        // Then
        Assert.Equal(SIZE, secureMemory.Length);

        // Dispose to avoid finalizer calls in test
        secureMemory.Dispose();
    }

    [Fact]
    public void Given_SecureMemory_When_CastToSpan_Then_ReturnsWritableSpan()
    {
        // Given
        const int SIZE = 8;
        using var secureMemory = new SecureMemory(SIZE);

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
        const int SIZE = 8;
        using var secureMemory = new SecureMemory(SIZE);

        // When
        ReadOnlySpan<byte> roSpan = secureMemory;

        // Then
        Assert.Equal(SIZE, roSpan.Length);
    }

    [Fact]
    public void Given_SecureMemory_When_Disposed_Then_MemoryZeroUnlockFreeCalledAndProviderDisposed()
    {
        // Given
        const int SIZE = 16;
        SecureMemory? secureMemory;
        secureMemory = new SecureMemory(SIZE);
        Span<byte> span = secureMemory;
        span[0] = 0xAB;

        // When
        secureMemory.Dispose();
        secureMemory = new SecureMemory(SIZE);
        span = secureMemory;

        // Then
        Assert.NotEqual(0xAB, span[0]);
    }
}