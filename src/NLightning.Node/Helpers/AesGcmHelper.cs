using System.Security.Cryptography;

namespace NLightning.Node.Helpers;

public static class AesGcmHelper
{
    private const int AES_GCM_TAG_SIZE = 16;

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var kdf = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return kdf.GetBytes(32);
    }

    public static byte[] Encrypt(byte[] plaintext, string password)
    {
        var salt = RandomNumberGenerator.GetBytes(AES_GCM_TAG_SIZE);
        var key = DeriveKey(password, salt);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[AES_GCM_TAG_SIZE];
        var ciphertext = new byte[plaintext.Length];

        using (var aes = new AesGcm(key, AES_GCM_TAG_SIZE))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        return salt.Concat(nonce).Concat(tag).Concat(ciphertext).ToArray();
    }

    public static byte[] Decrypt(byte[] encrypted, string password)
    {
        var salt = encrypted.AsSpan(0, AES_GCM_TAG_SIZE).ToArray();
        var nonce = encrypted.AsSpan(AES_GCM_TAG_SIZE, 12).ToArray();
        var tag = encrypted.AsSpan(28, AES_GCM_TAG_SIZE).ToArray();
        var ciphertext = encrypted.AsSpan(44).ToArray();
        var key = DeriveKey(password, salt);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, AES_GCM_TAG_SIZE);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return plaintext;
    }
}