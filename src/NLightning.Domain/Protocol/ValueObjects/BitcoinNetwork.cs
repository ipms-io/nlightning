namespace NLightning.Domain.Protocol.ValueObjects;

using Constants;

public readonly struct BitcoinNetwork : IEquatable<BitcoinNetwork>
{
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
                _ => throw new Exception("Chain not supported.")
            };
        }
    }

    public override string ToString() => Name;

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