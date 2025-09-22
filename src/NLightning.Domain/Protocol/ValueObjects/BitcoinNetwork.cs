namespace NLightning.Domain.Protocol.ValueObjects;

using Constants;

public readonly struct BitcoinNetwork : IEquatable<BitcoinNetwork>
{
    private static Dictionary<string, ChainHash> CustomChainHashes { get; } = new();

    public static readonly BitcoinNetwork Mainnet = new(NetworkConstants.Mainnet);
    public static readonly BitcoinNetwork Testnet = new(NetworkConstants.Testnet);
    public static readonly BitcoinNetwork Regtest = new(NetworkConstants.Regtest);
    public static readonly BitcoinNetwork Signet = new(NetworkConstants.Signet);

    public string Name { get; }

    public BitcoinNetwork(string name)
    {
        Name = name;
    }

    public ChainHash ChainHash
    {
        get
        {
            return Name switch
            {
                NetworkConstants.Mainnet => ChainConstants.Main,
                NetworkConstants.Testnet => ChainConstants.Testnet,
                NetworkConstants.Regtest => ChainConstants.Regtest,
                _ => CustomChainHashes.TryGetValue(Name.ToLowerInvariant(), out var hash)
                         ? hash
                         : throw new Exception($"Chain not supported: {Name}")
            };
        }
    }

    /// <summary>
    /// Register a custom network mapping
    /// </summary>
    public override string ToString() => Name;

    public void Register(string name, ChainHash chainHash)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

        CustomChainHashes.TryAdd(name.ToLowerInvariant(), chainHash);
    }

    /// <summary>
    /// Unregister a custom network mapping
    /// </summary>
    public static void Unregister(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
        CustomChainHashes.Remove(name.ToLowerInvariant());
    }

    #region Implicit Conversions

    public static implicit operator string(BitcoinNetwork bitcoinNetwork) => bitcoinNetwork.Name.ToLowerInvariant();
    public static implicit operator BitcoinNetwork(string value) => new(value.ToLowerInvariant());

    #endregion

    #region Equality

    public override bool Equals(object? obj)
    {
        return obj is BitcoinNetwork network && ChainHash.Equals(network.ChainHash);
    }

    public bool Equals(BitcoinNetwork other)
    {
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(BitcoinNetwork left, BitcoinNetwork right)
    {
        return left.Name == right.Name;
    }

    public static bool operator !=(BitcoinNetwork left, BitcoinNetwork right)
    {
        return !(left == right);
    }

    #endregion
}