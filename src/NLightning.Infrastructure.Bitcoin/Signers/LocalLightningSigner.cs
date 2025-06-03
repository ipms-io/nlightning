using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NLightning.Domain.Bitcoin.Interfaces;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Bitcoin.Signers;

public class LocalLightningSigner : ILightningSigner
{
    private readonly ISecureKeyManager _secureKeyManager;
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly ConcurrentDictionary<string, ChannelKeyData> _channelKeyData = new();
    private readonly ILogger<LocalLightningSigner> _logger;

    public LocalLightningSigner(IKeyDerivationService keyDerivationService, ILogger<LocalLightningSigner> logger,
                                ISecureKeyManager secureKeyManager)
    {
        _keyDerivationService = keyDerivationService;
        _logger = logger;
        _secureKeyManager = secureKeyManager;

        // TODO: Load channel key data from database
    }

    public SignedTransaction SignTransaction(SignedTransaction unsignedTx, LightningMoney feeRatePerKw)
    {
        throw new NotImplementedException();
    }

    public bool VerifySignature(SignedTransaction transaction, DerSignature derSignature, CompactPubKey pubKey, int index,
        LightningMoney amount, BitcoinScript? redeemScript = null)
    {
        throw new NotImplementedException();
    }

    public CompactPubKey GetPublicKey(string? keyIdentifier = null)
    {
        throw new NotImplementedException();
    }

    public CompactPubKey GenerateChannelKey()
    {
        throw new NotImplementedException();
    }

    public CompactPubKey DerivePerCommitmentPoint(ulong commitmentNumber)
    {
        throw new NotImplementedException();
    }

    public bool ValidateSignature(DerSignature payloadSignature)
    {
        throw new NotImplementedException();
    }
}