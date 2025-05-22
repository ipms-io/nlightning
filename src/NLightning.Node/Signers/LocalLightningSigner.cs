using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Node.Signers;

using Domain.Channels;
using Domain.Protocol.Managers;
using Domain.Protocol.Services;
using Domain.Protocol.Signers;

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

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public List<ECDSASignature> SignTransaction(Transaction transaction, IEnumerable<Coin> coins, SigHash sighash, params BitcoinSecret[] secrets)
    {
        throw new NotImplementedException();
    }

    public void AddKnownSignature(Transaction transaction, PubKey remotePubKey, ITransactionSignature remoteSignature,
        OutPoint outPoint)
    {
        throw new NotImplementedException();
    }

    public List<ECDSASignature> ExtractSignatures(Transaction transaction)
    {
        throw new NotImplementedException();
    }
}