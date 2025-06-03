using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.Constants;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Protocol.Payloads;
using NLightning.Infrastructure.Protocol.Services;

namespace NLightning.Infrastructure.Bitcoin.Factories;

public class ChannelKeySetFactory : IChannelKeySetFactory
{
    private readonly ISecureKeyManager _secureKeyManager;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly Network _network;

    public ChannelKeySetFactory(ISecureKeyManager secureKeyManager, IKeyDerivationService keyDerivationService,
                                IOptions<NodeOptions> nodeOptions)
    {
        _network = Network.GetNetwork(nodeOptions.Value.BitcoinNetwork)
                ?? throw new ArgumentException("Invalid Bitcoin network specified", nameof(nodeOptions));

        _secureKeyManager = secureKeyManager;
        _keyDerivationService = keyDerivationService;
    }

    public ChannelKeySet CreateNew()
    {
        // TODO: Have a better way to store unused indexes, like a pool that can be reused if the channel never confirms
        var channelPrivExtKey = _secureKeyManager.GetNextKey(out var index);
        var channelExtKey = ExtKey.CreateFromBytes(channelPrivExtKey);

        var firstPerCommitmentSecretBytes = _keyDerivationService
           .GeneratePerCommitmentSecret(channelExtKey.PrivateKey.ToBytes(), CryptoConstants.FirstPerCommitmentIndex);
        using var firstPerCommitmentSecret = new Key(firstPerCommitmentSecretBytes);
        using var localFundingSecret = channelExtKey.PrivateKey;
        using var localRevocationSecret = channelExtKey.Derive(0).PrivateKey;
        using var localPaymentSecret = channelExtKey.Derive(1).PrivateKey;
        using var localDelayedPaymentSecret = channelExtKey.Derive(2).PrivateKey;
        using var localHtlcSecret = channelExtKey.Derive(3).PrivateKey;

        // Generate Basepoints
        var localFirstPerCommitmentBasepoint = firstPerCommitmentSecret.PubKey;
        var localFundingPubKey = localFundingSecret.PubKey;
        var localRevocationBasepoint = localRevocationSecret.PubKey;
        var localPaymentBasepoint = localPaymentSecret.PubKey;
        var localDelayedPaymentBasepoint = localDelayedPaymentSecret.PubKey;
        var localHtlcBasepoint = localHtlcSecret.PubKey;

        var secretStorageService = new SecretStorageService();
        secretStorageService.InsertSecret(firstPerCommitmentSecret.ToBytes(), CryptoConstants.FirstPerCommitmentIndex);

        var keySet = new ChannelKeySet(index, localFundingPubKey.ToBytes(), localRevocationBasepoint.ToBytes(),
                                       localPaymentBasepoint.ToBytes(), localDelayedPaymentBasepoint.ToBytes(),
                                       localHtlcBasepoint.ToBytes(), null, localFirstPerCommitmentBasepoint.ToBytes(),
                                       CryptoConstants.FirstPerCommitmentIndex, secretStorageService);

        return keySet;
    }

    public ChannelKeySet CreateFromIndex(uint index)
    {
        throw new NotImplementedException();
    }

    public ChannelKeySet CreateFromRemoteInfo(OpenChannel1Payload payload)
    {
        return new ChannelKeySet(0, payload.FundingPubKey, payload.RevocationBasepoint, payload.PaymentBasepoint,
                                 payload.DelayedPaymentBasepoint, payload.HtlcBasepoint, null,
                                 payload.FirstPerCommitmentPoint, 0, new SecretStorageService());
    }
}