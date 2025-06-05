// using Microsoft.Extensions.Options;
// using NBitcoin;
// using NLightning.Domain.Channels.Models;
//
// namespace NLightning.Infrastructure.Bitcoin.Factories;
//
// using Domain.Protocol.Enums;
// using Domain.Channels.ValueObjects;
// using Domain.Crypto.Constants;
// using Domain.Node.Options;
// using Domain.Protocol.Interfaces;
// using Domain.Protocol.Payloads;
// using Infrastructure.Protocol.Services;
//
// public class ChannelKeySetFactory : IChannelKeySetFactory
// {
//     private readonly ISecureKeyManager _secureKeyManager;
//     private readonly IKeyDerivationService _keyDerivationService;
//     private readonly Network _network;
//
//     public ChannelKeySetFactory(ISecureKeyManager secureKeyManager, IKeyDerivationService keyDerivationService,
//                                 IOptions<NodeOptions> nodeOptions)
//     {
//         _network = Network.GetNetwork(nodeOptions.Value.BitcoinNetwork)
//                 ?? throw new ArgumentException("Invalid Bitcoin network specified", nameof(nodeOptions));
//
//         _secureKeyManager = secureKeyManager;
//         _keyDerivationService = keyDerivationService;
//     }
//
//     public ChannelKeySetModel CreateNew()
//     {
//         // Generate a new key for this channel
//         // TODO: Implement a proper pool or index management strategy
//         var channelPrivExtKey = _secureKeyManager.GetNextKey(out var index);
//         var channelExtKey = ExtKey.CreateFromBytes(channelPrivExtKey);
//
//         // Generate Lightning basepoints using proper BIP32 derivation paths
//         using var localFundingSecret = channelExtKey.Derive(0).PrivateKey; // m/0
//         using var localRevocationSecret = channelExtKey.Derive(1).PrivateKey; // m/1
//         using var localPaymentSecret = channelExtKey.Derive(2).PrivateKey; // m/2
//         using var localDelayedPaymentSecret = channelExtKey.Derive(3).PrivateKey; // m/3
//         using var localHtlcSecret = channelExtKey.Derive(4).PrivateKey; // m/4
//         using var perCommitmentSeed = channelExtKey.Derive(5).PrivateKey; // m/5
//
//         // Generate the first per-commitment secret and point
//         var firstPerCommitmentSecretBytes = _keyDerivationService
//            .GeneratePerCommitmentSecret(perCommitmentSeed.ToBytes(), CryptoConstants.FirstPerCommitmentIndex);
//         using var firstPerCommitmentSecret = new Key(firstPerCommitmentSecretBytes);
//         var firstPerCommitmentPoint = firstPerCommitmentSecret.PubKey;
//
//         // Generate static basepoints (these don't change per commitment)
//         var localFundingPubKey = localFundingSecret.PubKey;
//         var localRevocationBasepoint = localRevocationSecret.PubKey;
//         var localPaymentBasepoint = localPaymentSecret.PubKey;
//         var localDelayedPaymentBasepoint = localDelayedPaymentSecret.PubKey;
//         var localHtlcBasepoint = localHtlcSecret.PubKey;
//
//         // Create the secret storage service and store all secrets
//         var secretStorageService = new SecretStorageService();
//         secretStorageService.InsertSecret(firstPerCommitmentSecret.ToBytes(), CryptoConstants.FirstPerCommitmentIndex);
//         secretStorageService.StorePerCommitmentSeed(perCommitmentSeed.ToBytes());
//
//         // Store basepoint private keys
//         secretStorageService.StoreBasepointPrivateKey(BasepointType.Funding, localFundingSecret.ToBytes());
//         secretStorageService.StoreBasepointPrivateKey(BasepointType.Revocation, localRevocationSecret.ToBytes());
//         secretStorageService.StoreBasepointPrivateKey(BasepointType.Payment, localPaymentSecret.ToBytes());
//         secretStorageService.StoreBasepointPrivateKey(BasepointType.DelayedPayment,
//                                                       localDelayedPaymentSecret.ToBytes());
//         secretStorageService.StoreBasepointPrivateKey(BasepointType.Htlc, localHtlcSecret.ToBytes());
//
//         var keySet = new ChannelKeySetModel(index, localFundingPubKey.ToBytes(), localRevocationBasepoint.ToBytes(),
//                                        localPaymentBasepoint.ToBytes(), localDelayedPaymentBasepoint.ToBytes(),
//                                        localHtlcBasepoint.ToBytes(), null, firstPerCommitmentPoint.ToBytes(),
//                                        CryptoConstants.FirstPerCommitmentIndex, secretStorageService);
//
//         return keySet;
//     }
//
//     public ChannelKeySetModel CreateFromIndex(uint index)
//     {
//         // Recreate from the stored index
//         var channelPrivExtKey = _secureKeyManager.GetKeyAtIndex(index);
//         var channelExtKey = ExtKey.CreateFromBytes(channelPrivExtKey);
//
//         // Regenerate the same basepoints
//         using var localFundingSecret = channelExtKey.Derive(0).PrivateKey;
//         using var localRevocationSecret = channelExtKey.Derive(1).PrivateKey;
//         using var localPaymentSecret = channelExtKey.Derive(2).PrivateKey;
//         using var localDelayedPaymentSecret = channelExtKey.Derive(3).PrivateKey;
//         using var localHtlcSecret = channelExtKey.Derive(4).PrivateKey;
//         using var perCommitmentSeed = channelExtKey.Derive(5).PrivateKey;
//
//         // Regenerate first per-commitment point
//         var firstPerCommitmentSecretBytes = _keyDerivationService
//            .GeneratePerCommitmentSecret(perCommitmentSeed.ToBytes(), CryptoConstants.FirstPerCommitmentIndex);
//         using var firstPerCommitmentSecret = new Key(firstPerCommitmentSecretBytes);
//         var firstPerCommitmentPoint = firstPerCommitmentSecret.PubKey;
//
//         var localFundingPubKey = localFundingSecret.PubKey;
//         var localRevocationBasepoint = localRevocationSecret.PubKey;
//         var localPaymentBasepoint = localPaymentSecret.PubKey;
//         var localDelayedPaymentBasepoint = localDelayedPaymentSecret.PubKey;
//         var localHtlcBasepoint = localHtlcSecret.PubKey;
//
//         var secretStorageService = new SecretStorageService();
//         secretStorageService.LoadFromIndex(index); // Load existing secrets
//
//         var keySet = new ChannelKeySetModel(
//             index,
//             localFundingPubKey.ToBytes(),
//             localRevocationBasepoint.ToBytes(),
//             localPaymentBasepoint.ToBytes(),
//             localDelayedPaymentBasepoint.ToBytes(),
//             localHtlcBasepoint.ToBytes(),
//             null,
//             firstPerCommitmentPoint.ToBytes(),
//             CryptoConstants.FirstPerCommitmentIndex,
//             secretStorageService);
//
//         return keySet;
//     }
//
//     public ChannelKeySetModel CreateFromRemoteInfo(OpenChannel1Payload payload)
//     {
//         return new ChannelKeySetModel(0, payload.FundingPubKey, payload.RevocationBasepoint, payload.PaymentBasepoint,
//                                  payload.DelayedPaymentBasepoint, payload.HtlcBasepoint, null,
//                                  payload.FirstPerCommitmentPoint, 0, new SecretStorageService());
//     }
// }
//