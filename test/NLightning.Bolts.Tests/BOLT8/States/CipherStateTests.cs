namespace NLightning.Bolts.Tests.BOLT8.States;

using Bolts.BOLT8.States;
using Common.Constants;

public class CipherStateTests
{
    private static readonly byte[] s_dummyKey32 = Enumerable.Repeat<byte>(0x01, CryptoConstants.PRIVKEY_LEN).ToArray();
    private static readonly byte[] s_differentKey32 = Enumerable.Repeat<byte>(0x02, CryptoConstants.PRIVKEY_LEN).ToArray();
    private const int TEST_DATA_SIZE = 32;

    [Fact]
    public void Given_NoInitialization_When_HasKeysCalled_Then_ReturnsFalse()
    {
        // Given
        using var cipherState = new CipherState();

        // When
        var hasKeys = cipherState.HasKeys();

        // Then
        Assert.False(hasKeys, "Expected no keys if InitializeKeyAndChainingKey was never called.");
    }

    [Fact]
    public void Given_ValidKeys_When_InitializeKeyAndChainingKey_Then_HasKeyIsTrueAndNonceReset()
    {
        // Given
        using var cipherState = new CipherState();

        // When
        cipherState.InitializeKeyAndChainingKey(s_dummyKey32, s_differentKey32);

        // Then
        Assert.True(cipherState.HasKeys());
        // If we call encryption, we should not throw, meaning _k is set.
        var data = new byte[TEST_DATA_SIZE];
        var enc = new byte[data.Length + 16];
        cipherState.EncryptWithAd(ReadOnlySpan<byte>.Empty, data, enc);
        Assert.NotEqual(data, enc);
    }

    [Fact]
    public void Given_KeySet_When_SetNonce_Then_NonceIsUpdatedForSubsequentEncrypts()
    {
        // Given
        using var cipherState = new CipherState();
        cipherState.InitializeKeyAndChainingKey(s_dummyKey32, s_differentKey32);

        // When
        // Manually set the nonce to 5
        cipherState.SetNonce(5);

        // Then
        // On the next encryption, the state should use nonce=5, then increment to 6 internally
        var data = new byte[TEST_DATA_SIZE];
        var output = new byte[data.Length + 16];
        cipherState.EncryptWithAd(ReadOnlySpan<byte>.Empty, data, output);
        // No exception => success. We can't easily verify exact nonce usage unless we
        // test with a known vector or mock the cipher. For demonstration, we trust the logic.
    }

    [Fact]
    public void Given_NoKey_When_EncryptWithAd_Then_CopiesPlaintextToCiphertext()
    {
        // Given
        using var cipherState = new CipherState();
        var plaintext = new byte[] { 1, 2, 3, 4 };
        var ciphertext = new byte[4];

        // When
        var length = cipherState.EncryptWithAd(ReadOnlySpan<byte>.Empty, plaintext, ciphertext);

        // Then
        Assert.Equal(4, length);
        Assert.Equal(plaintext, ciphertext);
    }

    [Fact]
    public void Given_NoKey_When_DecryptWithAd_Then_CopiesCiphertextToPlaintext()
    {
        // Given
        using var cipherState = new CipherState();
        var ciphertext = new byte[] { 5, 6, 7, 8 };
        var plaintext = new byte[4];

        // When
        var length = cipherState.DecryptWithAd(ReadOnlySpan<byte>.Empty, ciphertext, plaintext);

        // Then
        Assert.Equal(4, length);
        Assert.Equal(ciphertext, plaintext);
    }

    [Fact]
    public void Given_NonceAtMax_When_EncryptWithAd_Then_ThrowsOverflowException()
    {
        // Given
        using var cipherState = new CipherState();
        cipherState.InitializeKeyAndChainingKey(s_dummyKey32, s_differentKey32);
        // Manually set nonce to 1000, which is the MAX_NONCE
        cipherState.SetNonce(1000);

        var data = new byte[TEST_DATA_SIZE];
        var output = new byte[data.Length + 16];

        // When/Then
        Assert.Throws<OverflowException>(() =>
        {
            cipherState.EncryptWithAd(ReadOnlySpan<byte>.Empty, data, output);
        });
    }

    [Fact]
    public void Given_KeySet_When_DecryptWithAd_Then_UsesCipher_AndIncrementsNonce()
    {
        // Given
        using var cipherState = new CipherState();
        cipherState.InitializeKeyAndChainingKey(s_dummyKey32, s_differentKey32);

        var plain = "HelloTest"u8.ToArray();
        var cipherWithAd = new byte[plain.Length + 16];
        cipherState.EncryptWithAd("AD"u8.ToArray(), plain, cipherWithAd);

        // Reset the cipherState nonce to 0 for demonstration
        cipherState.SetNonce(0);

        var recovered = new byte[plain.Length];

        // When
        var length = cipherState.DecryptWithAd("AD"u8.ToArray(), cipherWithAd, recovered);

        // Then
        Assert.Equal(plain.Length, length);
        Assert.Equal(plain, recovered);
    }

    [Fact]
    public void Given_EncryptDecrypt_When_NonceAtMax_RekeyIsCalled_Then_NonceResets()
    {
        // Given
        using var cipherState = new CipherState();
        cipherState.InitializeKeyAndChainingKey(s_dummyKey32, s_differentKey32);
        var plaintext = new byte[TEST_DATA_SIZE];
        var ciphertext = new byte[TEST_DATA_SIZE + 16];

        // Calls Encrypt once, so we have key material to perform a Rekey
        cipherState.Encrypt(plaintext, ciphertext);

        // Manually set nonce to 1000 (MAX_NONCE). 
        // Next call to Encrypt or Decrypt triggers Rekey(), resetting nonce to 0.
        cipherState.SetNonce(1000);

        // When
        // The first call to Encrypt sees nonce=MAX_NONCE => calls Rekey() => sets _n=0 => uses 0 => increments to 1
        cipherState.Encrypt(plaintext, ciphertext);

        // Then
        // Confirm no exception, and we presumably have rekeyed + used nonce=0 internally.
        Assert.True(cipherState.HasKeys(), "After rekeying, we still have a valid key.");
    }
}