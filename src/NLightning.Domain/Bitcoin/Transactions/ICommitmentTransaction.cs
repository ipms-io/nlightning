using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Domain.Bitcoin.Transactions;

using Outputs;
using Protocol.Signers;

public interface ICommitmentTransaction : ITransaction
{
    List<ECDSASignature> AppendRemoteSignatureAndSign(ILightningSigner signer, ECDSASignature remoteSignature,
        PubKey remotePubKey);
    void ReplaceFundingOutput(IFundingOutput oldFundingOutput, IFundingOutput newFundingOutput);
}