using System.Net;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT1.Primitives;

using Bolts.BOLT1.Primitives;

public class PeerAddressTests
{
    [Fact]
    public void Given_SingleStringAddress_When_ConstructingPeerAddress_Then_PropertiesAreCorrectlyInitialized()
    {
        // Given
        var address = "028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7@127.0.0.1:8080";

        // When
        var peerAddress = new PeerAddress(address);

        // Then
        Assert.Equal("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7", peerAddress.PubKey.ToString());
        Assert.Equal(IPAddress.Parse("127.0.0.1"), peerAddress.Host);
        Assert.Equal(8080, peerAddress.Port);
    }

    [Fact]
    public void Given_HttpAddress_When_ConstructingPeerAddress_Then_HostAndPortAreCorrectlyResolved()
    {
        // Given
        var pubKey = new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7");
        var address = "http://example.com:8080/";

        // When
        var peerAddress = new PeerAddress(pubKey, address);

        // Then
        Assert.Equal(pubKey, peerAddress.PubKey);
        Assert.Equal(IPAddress.Parse("93.184.216.34"), peerAddress.Host);
        Assert.Equal(8080, peerAddress.Port);
    }

    [Fact]
    public void Given_PubKeyHostAndPort_When_ConstructingPeerAddress_Then_PropertiesAreCorrectlyInitialized()
    {
        // Given
        var pubKey = new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7");
        var host = "127.0.0.1";
        var port = 8080;

        // When
        var peerAddress = new PeerAddress(pubKey, host, port);

        // Then
        Assert.Equal(pubKey, peerAddress.PubKey);
        Assert.Equal(IPAddress.Parse(host), peerAddress.Host);
        Assert.Equal(port, peerAddress.Port);
    }

    [Fact]
    public void Given_PeerAddressInstance_When_CallingToString_Then_ReturnsExpectedFormat()
    {
        // Given
        var pubKey = new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7");
        var host = "127.0.0.1";
        var port = 8080;
        var peerAddress = new PeerAddress(pubKey, host, port);

        // When
        var result = peerAddress.ToString();

        // Then
        Assert.Equal("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7@127.0.0.1:8080", result);
    }
}