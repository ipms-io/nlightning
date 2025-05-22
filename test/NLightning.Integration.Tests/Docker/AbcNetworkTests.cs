using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using Lnrpc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Application.Factories;
using NLightning.Tests.Utils;
using ServiceStack;
using ServiceStack.Text;
using Xunit.Abstractions;

namespace NLightning.Integration.Tests.Docker;

using Domain.Enums;
using Domain.Node.Options;
using Domain.Protocol.Constants;
using Fixtures;
using Infrastructure.Node.Managers;
using Infrastructure.Protocol.Factories;
using Infrastructure.Protocol.Models;
using Infrastructure.Serialization.Factories;
using Infrastructure.Serialization.Node;
using Infrastructure.Serialization.Tlv;
using Infrastructure.Transport.Factories;
using Mock;
using TestCollections;
using Utils;

// ReSharper disable AccessToDisposedClosure
#pragma warning disable xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
[Collection(LightningRegtestNetworkFixtureCollection.NAME)]
public class AbcNetworkTests
{
    private readonly LightningRegtestNetworkFixture _lightningRegtestNetworkFixture;

    public AbcNetworkTests(LightningRegtestNetworkFixture fixture, ITestOutputHelper output)
    {
        _lightningRegtestNetworkFixture = fixture;
        Console.SetOut(new TestOutputWriter(output));
    }

    [Fact]
    public async Task NLightning_BOLT8_Test_Connect_Alice()
    {
        // Arrange 
        var secureKeyManager = new FakeSecureKeyManager();
        var hex = Convert.ToHexString(secureKeyManager.GetNodeKey().PubKey.ToBytes());

        var alice = _lightningRegtestNetworkFixture.Builder?.LNDNodePool?.ReadyNodes.First(x => x.LocalAlias == "alice");
        Assert.NotNull(alice);

        var nodeOptions = new OptionsWrapper<NodeOptions>(new NodeOptions
        {
            Features = new FeatureOptions
            {
                ChainHashes = [ChainConstants.REGTEST],
                DataLossProtect = FeatureSupport.Optional,
                StaticRemoteKey = FeatureSupport.Optional,
                PaymentSecret = FeatureSupport.Optional
            }
        });
        var loggerFactory = new LoggerFactory();
        var messageFactory = new MessageFactory(nodeOptions);
        var valueObjectSerializerFactory = new ValueObjectSerializerFactory();
        var tlvConverterFactory = new TlvConverterFactory();
        var messageTypeSerializerFactory = new MessageTypeSerializerFactory(
            new PayloadSerializerFactory(new FeatureSetSerializer(), valueObjectSerializerFactory),
                                         tlvConverterFactory,
                                         new TlvStreamSerializer(tlvConverterFactory, new TlvSerializer(valueObjectSerializerFactory)));
        var messageSerializer =
            new Infrastructure.Serialization.Messages.MessageSerializer(messageTypeSerializerFactory);
        var peerManager = new PeerManager(new Mock<ILogger<PeerManager>>().Object, nodeOptions,
            new PeerServiceFactory(loggerFactory, messageFactory, new MessageServiceFactory(messageSerializer),
                            new PingPongServiceFactory(messageFactory, nodeOptions), secureKeyManager,
                            new TransportServiceFactory(loggerFactory, messageSerializer, nodeOptions), nodeOptions));

        var aliceHost = new IPEndPoint((await Dns.GetHostAddressesAsync(alice.Host
                                                                             .SplitOnFirst("//")[1]
                                                                             .SplitOnFirst(":")[0])).First(), 9735);

        // Act
        await peerManager.ConnectToPeerAsync(new PeerAddress(new PubKey(alice.LocalNodePubKeyBytes),
                                                                        aliceHost.Address.ToString(),
                                                                        aliceHost.Port));
        var alicePeers = alice.LightningClient.ListPeers(new ListPeersRequest());

        // Assert
        Assert.NotNull(alicePeers.Peers.FirstOrDefault(x => x.PubKey
                                                             .Equals(hex, StringComparison.CurrentCultureIgnoreCase)));

        // Cleanup
        peerManager.DisconnectPeer(new PubKey(alice.LocalNodePubKeyBytes));
    }

    [Fact]
    public async Task NLightning_BOLT8_Test_Bob_Connect()
    {
        // Arrange
        var secureKeyManager = new FakeSecureKeyManager();
        var availablePort = await PortPoolUtil.GetAvailablePortAsync();
        var listener = new TcpListener(IPAddress.Any, availablePort);
        listener.Start();

        try
        {
            // Get ip from host
            var hostAddress = Environment.GetEnvironmentVariable("HOST_ADDRESS") ?? "host.docker.internal";

            var hex = Convert.ToHexString(secureKeyManager.GetNodeKey().PubKey.ToBytes());

            var bob = _lightningRegtestNetworkFixture
                .Builder?
                .LNDNodePool?
                .ReadyNodes.First(x => x.LocalAlias == "bob");
            Assert.NotNull(bob);

            var nodeOptions = new OptionsWrapper<NodeOptions>(new NodeOptions
            {
                Features = new FeatureOptions
                {
                    ChainHashes = [ChainConstants.REGTEST],
                    DataLossProtect = FeatureSupport.Optional,
                    StaticRemoteKey = FeatureSupport.Optional,
                    PaymentSecret = FeatureSupport.Optional
                }
            });
            var loggerFactory = new LoggerFactory();
            var messageFactory = new MessageFactory(nodeOptions);
            var valueObjectSerializerFactory = new ValueObjectSerializerFactory();
            var tlvConverterFactory = new TlvConverterFactory();
            var messageTypeSerializerFactory = new MessageTypeSerializerFactory(
                new PayloadSerializerFactory(new FeatureSetSerializer(), valueObjectSerializerFactory),
                tlvConverterFactory,
                new TlvStreamSerializer(tlvConverterFactory, new TlvSerializer(valueObjectSerializerFactory)));
            var messageSerializer =
                new Infrastructure.Serialization.Messages.MessageSerializer(messageTypeSerializerFactory);
            var peerManager = new PeerManager(new Mock<ILogger<PeerManager>>().Object, nodeOptions,
                new PeerServiceFactory(loggerFactory, messageFactory, new MessageServiceFactory(messageSerializer),
                    new PingPongServiceFactory(messageFactory, nodeOptions), secureKeyManager,
                    new TransportServiceFactory(loggerFactory, messageSerializer, nodeOptions), nodeOptions));

            var acceptTask = Task.Run(async () =>
            {
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();

                    await peerManager.AcceptPeerAsync(tcpClient);
                }
            });
            await Task.Delay(1000);

            // Act
            await bob.LightningClient.ConnectPeerAsync(new ConnectPeerRequest
            {
                Addr = new LightningAddress
                {
                    Host = $"{hostAddress}:{availablePort}",
                    Pubkey = hex
                }
            });
            var alicePeers = bob.LightningClient.ListPeers(new ListPeersRequest());
            await acceptTask;

            // Assert
            Assert.NotNull(alicePeers.Peers
                .FirstOrDefault(x => x.PubKey.Equals(hex, StringComparison.CurrentCultureIgnoreCase)));

            // Cleanup
            peerManager.DisconnectPeer(new PubKey(bob.LocalNodePubKeyBytes));
        }
        finally
        {
            listener.Dispose();
            PortPoolUtil.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Verify_Alice_Bob_Carol_Setup()
    {
        var readyNodes = _lightningRegtestNetworkFixture.Builder!.LNDNodePool!.ReadyNodes.ToImmutableList();
        var nodeCount = readyNodes.Count;
        Assert.Equal(3, nodeCount);
        $"LND Nodes in Ready State: {nodeCount}".Print();
        foreach (var node in readyNodes)
        {
            var walletBalanceResponse = await node.LightningClient.WalletBalanceAsync(new WalletBalanceRequest());
            var channels = await node.LightningClient.ListChannelsAsync(new ListChannelsRequest());
            $"Node {node.LocalAlias} ({node.LocalNodePubKey})".Print();
            walletBalanceResponse.PrintDump();
            channels.PrintDump();
        }

        $"Bitcoin Node Balance: {(await _lightningRegtestNetworkFixture.Builder!.BitcoinRpcClient!.GetBalanceAsync()).Satoshi / 1e8}".Print();
    }
}
#pragma warning restore xUnit1033 // Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
// ReSharper restore AccessToDisposedClosure