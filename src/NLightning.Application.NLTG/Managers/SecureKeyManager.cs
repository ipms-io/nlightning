using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using NBitcoin;
using Serilog;

namespace NLightning.Application.NLTG.Managers;

using Constants;
using Domain.Crypto.Constants;
using Domain.Protocol.Managers;
using Infrastructure.Crypto.Ciphers;
using Infrastructure.Crypto.Factories;
using Infrastructure.Crypto.Hashes;
using Models;

/// <summary>
/// Manages a securely stored private key using protected memory allocation.
/// This class ensures that the private key remains inaccessible from regular memory
/// and is securely wiped when no longer needed.
/// </summary>
public class SecureKeyManager : ISecureKeyManager, IDisposable
{
    private readonly string _filePath;
    private readonly object _lastUsedIndexLock = new();
    private readonly Network _network = Network.Main;

    private uint _lastUsedIndex;
    private ulong _privateKeyLength;
    private IntPtr _securePrivateKeyPtr;

    public const string PATH = "m/6425'/0'/0'/0/{0}";

    public string OutputDescriptor { get; init; }

    /// <summary>
    /// Manages secure key operations for generating and managing cryptographic keys.
    /// Provides functionality to safely store, load, and derive secure keys protected in memory.
    /// </summary>
    /// <param name="privateKey">The private key to be managed.</param>
    /// <param name="network">The network associated with the private key.</param>
    /// <param name="filePath">The file path for storing the key data.</param>
    public SecureKeyManager(byte[] privateKey, Network network, string filePath)
    {
        _privateKeyLength = (ulong)privateKey.Length;

        using var cryptoProvider = CryptoFactory.GetCryptoProvider();

        // Allocate secure memory
        _securePrivateKeyPtr = cryptoProvider.MemoryAlloc(_privateKeyLength);

        // Lock the memory to prevent swapping
        if (cryptoProvider.MemoryLock(_securePrivateKeyPtr, _privateKeyLength) == -1)
            throw new InvalidOperationException("Failed to lock memory.");

        // Copy the private key to secure memory
        Marshal.Copy(privateKey, 0, _securePrivateKeyPtr, (int)_privateKeyLength);

        // Get Output Descriptor
        var extKey = new ExtKey(new Key(privateKey), network.GenesisHash.ToBytes());
        var xpub = extKey.Neuter().ToString(_network);
        var fingerprint = extKey.GetPublicKey().GetHDFingerPrint();

        OutputDescriptor = $"wpkh([{fingerprint}/{string.Format(PATH, "*")}]{xpub}/0/*)";

        // Securely wipe the original key from regular memory
        cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(privateKey, 0), _privateKeyLength);

        _filePath = filePath;
        _network = network;
    }

    public ExtKey GetNextKey(out uint index)
    {
        lock (_lastUsedIndexLock)
        {
            _lastUsedIndex++;
            index = _lastUsedIndex;
        }

        // Derive the key at m/6425'/0'/0'/0/index
        var masterKey = GetMasterKey();
        var derivedKey = masterKey.Derive(new KeyPath(string.Format(PATH, index)));

        _ = UpdateLastUsedIndexOnFile().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Log.Error(task.Exception, "Failed to update last used index on file");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);

        return derivedKey;
    }

    public ExtKey GetKeyAtIndex(uint index)
    {
        var masterKey = GetMasterKey();
        return masterKey.Derive(new KeyPath(string.Format(PATH, index)));
    }

    public Key GetNodeKey()
    {
        var masterKey = GetMasterKey();
        return masterKey.PrivateKey;
    }

    public PubKey GetNodePubKey()
    {
        var masterKey = GetMasterKey();
        return masterKey.PrivateKey.PubKey;
    }

    public async Task UpdateLastUsedIndexOnFile()
    {
        var jsonString = File.ReadAllText(_filePath);
        var data = JsonSerializer.Deserialize<KeyFileData>(jsonString)
                   ?? throw new SerializationException("Invalid key file");

        lock (_lastUsedIndexLock)
        {
            data.LastUsedIndex = _lastUsedIndex;
        }

        jsonString = JsonSerializer.Serialize(data);

        await File.WriteAllTextAsync(_filePath, jsonString);
    }

    public void SaveToFile(string password)
    {
        lock (_lastUsedIndexLock)
        {
            var extKey = GetMasterKey();
            var extKeyBytes = Encoding.UTF8.GetBytes(extKey.ToString(_network));

            Span<byte> salt = stackalloc byte[CryptoConstants.XCHACHA20_POLY1305_TAG_LEN];
            Span<byte> key = stackalloc byte[CryptoConstants.PRIVKEY_LEN];
            Span<byte> nonce = stackalloc byte[CryptoConstants.XCHACHA20_POLY1305_NONCE_LEN];
            Span<byte> cipherText = stackalloc byte[extKeyBytes.Length + CryptoConstants.XCHACHA20_POLY1305_TAG_LEN];

            using var argon2Id = new Argon2Id();
            argon2Id.DeriveKeyFromPasswordAndSalt(password, salt, key);

            using var xChaCha20Poly1305 = new XChaCha20Poly1305();
            xChaCha20Poly1305.Encrypt(key, nonce, ReadOnlySpan<byte>.Empty, extKeyBytes, cipherText);

            var data = new KeyFileData
            {
                Network = _network.ToString(),
                LastUsedIndex = _lastUsedIndex,
                Descriptor = OutputDescriptor,
                EncryptedExtKey = Convert.ToBase64String(cipherText),
                Nonce = Convert.ToBase64String(nonce),
                Salt = Convert.ToBase64String(salt)
            };
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(_filePath, json);
        }
    }

    public static SecureKeyManager FromMnemonic(string mnemonic, string passphrase, Network network,
                                                string? filePath = null)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            filePath = GetKeyFilePath(network.ToString());

        var mnemonicObj = new Mnemonic(mnemonic, Wordlist.English);
        var extKey = mnemonicObj.DeriveExtKey(passphrase);
        return new SecureKeyManager(extKey.PrivateKey.ToBytes(), network, filePath);
    }

    public static SecureKeyManager FromFilePath(string filePath, Network expectedNetwork, string password)
    {
        var jsonString = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<KeyFileData>(jsonString)
                   ?? throw new SerializationException("Invalid key file");

        var network = Network.GetNetwork(data.Network) ?? throw new Exception("Invalid network");
        if (expectedNetwork != network)
            throw new Exception($"Invalid network. Expected {expectedNetwork}, but got {network}");

        var encryptedExtKey = Convert.FromBase64String(data.EncryptedExtKey);
        var nonce = Convert.FromBase64String(data.Nonce);
        var salt = Convert.FromBase64String(data.Salt);

        Span<byte> key = stackalloc byte[CryptoConstants.PRIVKEY_LEN];
        using var argon2Id = new Argon2Id();
        argon2Id.DeriveKeyFromPasswordAndSalt(password, salt, key);

        Span<byte> extKeyBytes = stackalloc byte[encryptedExtKey.Length - CryptoConstants.XCHACHA20_POLY1305_TAG_LEN];
        using var xChaCha20Poly1305 = new XChaCha20Poly1305();
        xChaCha20Poly1305.Decrypt(key, nonce, ReadOnlySpan<byte>.Empty, encryptedExtKey, extKeyBytes);

        var extKeyStr = Encoding.UTF8.GetString(extKeyBytes);
        var extKey = ExtKey.Parse(extKeyStr, network);

        return new SecureKeyManager(extKey.PrivateKey.ToBytes(), network, filePath)
        {
            _lastUsedIndex = data.LastUsedIndex,
            OutputDescriptor = data.Descriptor
        };
    }

    /// <summary>
    /// Gets the path for the Key file
    /// </summary>
    public static string GetKeyFilePath(string network)
    {
        var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var networkDir = Path.Combine(homeDir, ".nltg", network);
        Directory.CreateDirectory(networkDir); // Ensure directory exists
        return Path.Combine(networkDir, DaemonConstants.KEY_FILE);
    }

    private ExtKey GetMasterKey()
    {
        return new ExtKey(new Key(GetPrivateKeyBytes()), _network.GenesisHash.ToBytes());
    }

    private void ReleaseUnmanagedResources()
    {
        if (_securePrivateKeyPtr == IntPtr.Zero)
            return;

        using var cryptoProvider = CryptoFactory.GetCryptoProvider();

        // Securely wipe the memory before freeing it
        cryptoProvider.MemoryZero(_securePrivateKeyPtr, _privateKeyLength);

        // Unlock the memory
        cryptoProvider.MemoryUnlock(_securePrivateKeyPtr, _privateKeyLength);

        // MemoryFree the memory
        cryptoProvider.MemoryFree(_securePrivateKeyPtr);

        _privateKeyLength = 0;
        _securePrivateKeyPtr = IntPtr.Zero;
    }

    /// <summary>
    /// Retrieves the private key stored in secure memory.
    /// </summary>
    /// <returns>The private key as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the key is not initialized.</exception>
    private byte[] GetPrivateKeyBytes()
    {
        if (_securePrivateKeyPtr == IntPtr.Zero)
            throw new InvalidOperationException("Secure key is not initialized.");

        var privateKey = new byte[_privateKeyLength];
        Marshal.Copy(_securePrivateKeyPtr, privateKey, 0, (int)_privateKeyLength);

        return privateKey;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SecureKeyManager()
    {
        ReleaseUnmanagedResources();
    }
}