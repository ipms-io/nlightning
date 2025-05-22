using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Domain.Protocol.Signers;

/// <summary>
/// Interface for Lightning compatible signers that handle key management, channel basepoints,
/// and transaction signing operations
/// </summary>
public interface ILightningSigner : IDisposable
{
    List<ECDSASignature> SignTransaction(Transaction transaction, IEnumerable<Coin> coins, SigHash sighash,
                                         params BitcoinSecret[] secrets);
    void AddKnownSignature(Transaction transaction, PubKey remotePubKey, ITransactionSignature remoteSignature,
                           OutPoint outPoint);

    List<ECDSASignature> ExtractSignatures(Transaction transaction);
}