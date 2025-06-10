// using System.Reflection;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using NBitcoin;
//
// namespace NLightning.Infrastructure.Tests.Node.Models;
//
// using Domain.Enums;
// using Domain.Exceptions;
// using Domain.Node.Options;
// using Domain.Protocol.Constants;
// using Domain.Protocol.Messages;
// using Domain.Protocol.Payloads;
// using Domain.Protocol.Services;
// using Infrastructure.Protocol.Models;
//
// public class PeerTests
// {
//     private static readonly Mock<IMessageService> s_mockMessageService = new();
//     private static readonly Mock<IPingPongService> s_mockPingPongService = new();
//     private static readonly Mock<ILogger<Peer>> s_mockLogger = new();
//     private static readonly NodeOptions s_nodeOptions = new();
//     private static readonly MessageFactory s_messageFactory = new(Options.Create(s_nodeOptions));
//     private static readonly PeerAddress s_peerAddress = new(new Key().PubKey, "127.0.0.1", 1234);
//
//     [Fact]
//     public void Given_OutboundPeer_When_Constructing_Then_InitMessageIsSent()
//     {
//         // Arrange
//         s_mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<InitMessage>(), It.IsAny<CancellationToken>()))
//                            .Returns(Task.CompletedTask)
//                            .Verifiable();
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         // Act
//         _ = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                      s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//
//         // Assert
//         s_mockMessageService.Verify();
//     }
//
//     [Fact]
//     public void Given_MessageServiceIsNotConnected_When_PeerIsConstructed_Then_ThrowsException()
//     {
//         // Arrange
//         // Simulate the message service being disconnected
//         s_mockMessageService.Setup(m => m.IsConnected).Returns(false);
//
//         // Act & Assert
//         var exception = Assert.Throws<ConnectionException>(() =>
//             new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                      s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object)
//         );
//
//         Assert.Equal("Failed to connect to peer", exception.Message);
//     }
//
//     [Fact]
//     public async Task Given_InboundPeer_When_InitMessageIsNotReceivedWithinTimeout_Then_Disconnects()
//     {
//         // Arrange
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             TimeSpan.FromSeconds(1), s_peerAddress, s_mockPingPongService.Object);
//
//         // Act & Assert
//         await Assert.RaisesAnyAsync(
//             e => peer.DisconnectEvent += e,
//             e => peer.DisconnectEvent -= e,
//             async () => await Task.Delay(TimeSpan.FromSeconds(2)));
//     }
//
//     [Fact]
//     public void Given_InboundPeer_When_ReceivingValidInitMessage_Then_IsInitialized()
//     {
//         // Arrange
//         var initMessage = s_messageFactory.CreateInitMessage();
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//
//         // Act
//         var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(method);
//         method.Invoke(peer, [peer, initMessage]);
//
//         // Assert
//         var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
//         var value = field?.GetValue(peer);
//         Assert.NotNull(value);
//         Assert.True((bool)value);
//     }
//
//     [Fact]
//     public void Given_InboundPeer_When_ReceivingInvalidInitMessage_Then_Disconnects()
//     {
//         // Arrange
//         var disconnectEventRaised = false;
//         var pingMessage = s_messageFactory.CreatePingMessage();
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//         peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;
//
//         // Act
//         var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(method);
//         method.Invoke(peer, [peer, pingMessage]);
//
//         // Assert
//         Assert.True(disconnectEventRaised);
//     }
//
//     [Fact]
//     public void Given_InboundPeer_When_ReceivingIncompatibleFeatures_Then_Disconnects()
//     {
//         // Arrange
//         var disconnectEventRaised = false;
//         var features = s_nodeOptions.Features.GetNodeFeatures();
//         features.SetFeature(Feature.OptionZeroconf, true);
//         var initMessage = new InitMessage(new InitPayload(features), s_nodeOptions.Features.GetInitTlvs());
//
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//         peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;
//
//         // Act
//         var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(method);
//         method.Invoke(peer, [peer, initMessage]);
//
//         // Assert
//         Assert.True(disconnectEventRaised);
//     }
//
//     [Fact]
//     public void Given_InboundPeer_When_ReceivingIncompatibleChain_Then_Disconnects()
//     {
//         // Arrange
//         var disconnectEventRaised = false;
//         var otherNodeOptions = new NodeOptions
//         {
//             Features = new FeatureOptions
//             {
//                 ChainHashes = [ChainConstants.Regtest]
//             }
//         };
//         var otherMessageFactory = new MessageFactory(Options.Create(otherNodeOptions));
//         var initMessage = otherMessageFactory.CreateInitMessage();
//
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//         peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;
//
//         // Act
//         var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(method);
//         method.Invoke(peer, [peer, initMessage]);
//
//         // Assert
//         Assert.True(disconnectEventRaised);
//     }
//
//     [Fact]
//     public void Given_Peer_When_ReceivingPingMessage_Then_SendsPongMessage()
//     {
//         // Arrange
//         var pingMessage = s_messageFactory.CreatePingMessage();
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         s_mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<PongMessage>(), It.IsAny<CancellationToken>()))
//             .Returns(Task.CompletedTask)
//             .Verifiable();
//
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//
//         var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(field);
//         field.SetValue(peer, true);
//
//         // Act
//         var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(method);
//         method.Invoke(peer, [peer, pingMessage]);
//
//         // Assert
//         s_mockMessageService.Verify();
//     }
//
//     [Fact]
//     public void Given_Peer_When_ReceivingPongMessage_Then_PingPongServiceHandlesPong()
//     {
//         // Arrange
//         var pongMessage = s_messageFactory.CreatePongMessage(new PingMessage(new PingPayload { NumPongBytes = 1 }));
//         s_mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
//
//         var peer = new Peer(s_nodeOptions.Features, s_mockLogger.Object, s_messageFactory, s_mockMessageService.Object,
//                             s_nodeOptions.NetworkTimeout, s_peerAddress, s_mockPingPongService.Object);
//
//         var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(field);
//         field.SetValue(peer, true);
//
//         // Act
//         var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
//         Assert.NotNull(method);
//         method.Invoke(peer, [peer, pongMessage]);
//
//         // Assert
//         s_mockPingPongService.Verify(p => p.HandlePong(It.IsAny<PongMessage>()), Times.Once());
//     }
// }