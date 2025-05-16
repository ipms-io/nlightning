namespace NLightning.Domain.Tests.ValueObjects;

using Domain.Protocol.Constants;
using Domain.ValueObjects;

public class NetworkTests
{
    [Fact]
    public void Given_NetworkInstances_When_ComparedForEquality_Then_ReturnsCorrectResult()
    {
        // Given
        var mainNet1 = Network.MAINNET;
        var mainNet2 = new Network("mainnet");
        var testNet = Network.TESTNET;

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
        var network = Network.MAINNET;

        // When
        string networkName = network;

        // Then
        Assert.Equal("mainnet", networkName);
    }

    [Fact]
    public void Given_String_When_ConvertedToNetwork_Then_ReturnsCorrectNetwork()
    {
        // Given
        const string NETWORK_NAME = "testnet";

        // When
        Network network = NETWORK_NAME;

        // Then
        Assert.Equal(Network.TESTNET, network);
    }

    [Fact]
    public void Given_NetworkInstance_When_ConvertedToNBitcoinNetwork_Then_ReturnsCorrectNBitcoinNetwork()
    {
        // Given
        var network = Network.MAINNET;

        // When
        var nBitcoinNetwork = (NBitcoin.Network)network;

        // Then
        Assert.Equal(NBitcoin.Network.Main, nBitcoinNetwork);
    }

    [Fact]
    public void Given_NBitcoinNetwork_When_ConvertedToNetwork_Then_ReturnsCorrectNetwork()
    {
        // Given
        var nBitcoinNetwork = NBitcoin.Network.TestNet;

        // When
        Network network = nBitcoinNetwork;

        // Then
        Assert.Equal(Network.TESTNET, network);
    }

    [Theory]
    [InlineData(NetworkConstants.MAINNET)]
    [InlineData(NetworkConstants.TESTNET)]
    [InlineData(NetworkConstants.REGTEST)]
    public void Given_NetworkInstance_When_ChainHashAccessed_Then_ReturnsCorrectHash(string networkName)
    {
        // Given
        var network = new Network(networkName);
        var expectedChain = networkName switch
        {
            NetworkConstants.MAINNET => ChainConstants.MAIN,
            NetworkConstants.TESTNET => ChainConstants.TESTNET,
            NetworkConstants.REGTEST => ChainConstants.REGTEST,
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
        var network = new Network("unsupported");

        // When & Then
        Assert.Throws<Exception>(() => network.ChainHash);
    }

    [Fact]
    public void Given_NetworkInstance_When_ToStringCalled_Then_ReturnsCorrectName()
    {
        // Given
        var network = Network.MAINNET;

        // When
        var networkName = network.ToString();

        // Then
        Assert.Equal("mainnet", networkName);
    }

    [Fact]
    public void Given_TwoEqualNetworkInstances_When_Compared_Then_AreEqual()
    {
        // Given
        var network1 = new Network("mainnet");
        var network2 = Network.MAINNET;

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
        var network1 = Network.MAINNET;
        var network2 = Network.TESTNET;

        // When & Then
        Assert.False(network1 == network2);
        Assert.True(network1 != network2);
        Assert.False(network1.Equals(network2));
        Assert.False(network2.Equals(network1));
    }
}