namespace NLightning.Domain.ValueObjects;

using Protocol.Constants;

public readonly struct Network : IEquatable<Network>, IEquatable<NBitcoin.Network>
{
    public static readonly Network MAINNET = new(NetworkConstants.MAINNET);
    public static readonly Network TESTNET = new(NetworkConstants.TESTNET);
    public static readonly Network REGTEST = new(NetworkConstants.REGTEST);
    public static readonly Network SIGNET = new(NetworkConstants.SIGNET);

    public string Name { get; }

    public Network(string name)
    {
        Name = name;
    }

    public ChainHash ChainHash
    {
        get
        {
            return Name switch
            {
                NetworkConstants.MAINNET => ChainConstants.MAIN,
                NetworkConstants.TESTNET => ChainConstants.TESTNET,
                NetworkConstants.REGTEST => ChainConstants.REGTEST,
                _ => throw new Exception("Chain not supported.")
            };
        }
    }

    public override string ToString() => Name;

    #region Implicit Conversions
    public static implicit operator string(Network network) => network.Name;
    public static implicit operator Network(string value) => new(value);

    public static implicit operator NBitcoin.Network(Network network)
    {
        return network.Name switch
        {
            NetworkConstants.MAINNET => NBitcoin.Network.Main,
            NetworkConstants.TESTNET => NBitcoin.Network.TestNet,
            NetworkConstants.REGTEST => NBitcoin.Network.RegTest,
            _ => throw new ArgumentException("Unsupported network type", nameof(network)),
        };
    }
    public static implicit operator Network(NBitcoin.Network network)
    {
        return network.Name switch
        {
            "Main" => MAINNET,
            "TestNet" => TESTNET,
            "RegTest" => REGTEST,
            _ => throw new ArgumentException("Unsupported network type", nameof(network)),
        };
    }
    #endregion

    #region Equality
    public override bool Equals(object? obj)
    {
        return obj is Network network && Name == network.Name;
    }

    public bool Equals(Network other)
    {
        return Name == other.Name;
    }

    public bool Equals(NBitcoin.Network? other)
    {
        return other is not null && ((byte[])ChainHash).SequenceEqual(other.GenesisHash.ToBytes());
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public static bool operator ==(Network left, Network right)
    {
        return left.Name == right.Name;
    }

    public static bool operator !=(Network left, Network right)
    {
        return !(left == right);
    }
    #endregion
}