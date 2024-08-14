namespace NLightning.Common.Types;

using Constants;

public readonly struct Network(string name)
{
    public static readonly Network MAIN_NET = new(NetworkConstants.MAINNET);
    public static readonly Network TEST_NET = new(NetworkConstants.TESTNET);
    public static readonly Network REG_TEST = new(NetworkConstants.REGTEST);
    public static readonly Network SIG_NET = new(NetworkConstants.SIGNET);

    public string Name { get; } = name;

    public override string ToString() => Name;

    #region Implicit Conversions
    public static implicit operator string(Network network) => network.Name;
    public static implicit operator Network(string value) => new(value);
    #endregion

    #region Equality
    public override bool Equals(object? obj)
    {
        return obj is Network network && Name == network.Name;
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