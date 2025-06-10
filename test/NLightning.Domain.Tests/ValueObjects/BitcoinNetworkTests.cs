using NLightning.Domain.Protocol.ValueObjects;

namespace NLightning.Domain.Tests.ValueObjects;

using Domain.Protocol.Constants;

public class BitcoinNetworkTests
{
    [Fact]
    public void Given_NetworkInstances_When_ComparedForEquality_Then_ReturnsCorrectResult()
    {
        // Given
        var mainNet1 = BitcoinNetwork.Mainnet;
        var mainNet2 = new BitcoinNetwork("mainnet");
        var testNet = BitcoinNetwork.Testnet;

        // When & Then
        Assert.True(mainNet1 == mainNet2);
        Assert.False(mainNet1 == testNet);
        Assert.True(mainNet1.Equals(mainNet2));
        Assert.False(mainNet1.Equals(testNet));
    }

    [Fact]
    public void Given_NetworkInstance_When_ConvertedToString_Then_ReturnsCorrectName()
    {
        // Given
        var network = BitcoinNetwork.Mainnet;

        // When
        string networkName = network;

        // Then
        Assert.Equal("mainnet", networkName);
    }

    [Fact]
    public void Given_String_When_ConvertedToNetwork_Then_ReturnsCorrectNetwork()
    {
        // Given
        const string networkName = "testnet";

        // When
        BitcoinNetwork bitcoinNetwork = networkName;

        // Then
        Assert.Equal(BitcoinNetwork.Testnet, bitcoinNetwork);
    }

    [Theory]
    [InlineData(NetworkConstants.Mainnet)]
    [InlineData(NetworkConstants.Testnet)]
    [InlineData(NetworkConstants.Regtest)]
    public void Given_NetworkInstance_When_ChainHashAccessed_Then_ReturnsCorrectHash(string networkName)
    {
        // Given
        var network = new BitcoinNetwork(networkName);
        var expectedChain = networkName switch
        {
            NetworkConstants.Mainnet => ChainConstants.Main,
            NetworkConstants.Testnet => ChainConstants.Testnet,
            NetworkConstants.Regtest => ChainConstants.Regtest,
            _ => throw new Exception("Chain not supported.")
        };

        // When
        var chainHash = network.ChainHash;

        // Then
        Assert.Equal(expectedChain, chainHash);
    }

    [Fact]
    public void Given_UnsupportedNetworkName_When_ChainHashAccessed_Then_ThrowsException()
    {
        // Given
        var network = new BitcoinNetwork("unsupported");

        // When & Then
        Assert.Throws<Exception>(() => network.ChainHash);
    }

    [Fact]
    public void Given_NetworkInstance_When_ToStringCalled_Then_ReturnsCorrectName()
    {
        // Given
        var network = BitcoinNetwork.Mainnet;

        // When
        var networkName = network.ToString();

        // Then
        Assert.Equal("mainnet", networkName);
    }

    [Fact]
    public void Given_TwoEqualNetworkInstances_When_Compared_Then_AreEqual()
    {
        // Given
        var network1 = new BitcoinNetwork("mainnet");
        var network2 = BitcoinNetwork.Mainnet;

        // When & Then
        Assert.True(network1 == network2);
        Assert.False(network1 != network2);
        Assert.True(network1.Equals(network2));
        Assert.True(network2.Equals(network1));
    }

    [Fact]
    public void Given_TwoDifferentNetworkInstances_When_Compared_Then_AreNotEqual()
    {
        // Given
        var network1 = BitcoinNetwork.Mainnet;
        var network2 = BitcoinNetwork.Testnet;

        // When & Then
        Assert.False(network1 == network2);
        Assert.True(network1 != network2);
        Assert.False(network1.Equals(network2));
        Assert.False(network2.Equals(network1));
    }
}