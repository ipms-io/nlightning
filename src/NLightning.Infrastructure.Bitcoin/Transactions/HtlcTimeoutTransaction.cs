// using NBitcoin;
// using NBitcoin.Crypto;
//
// namespace NLightning.Infrastructure.Bitcoin.Transactions;
//
// using Domain.Protocol.Signers;
// using Outputs;
//
// public class HtlcTimeoutTransaction : BaseHtlcTransaction
// {
//     public HtlcTimeoutTransaction(bool hasAnchorOutputs, Network network, BaseHtlcOutput output,
//                                    PubKey revocationPubKey, PubKey localDelayedPubKey, uint cltvEpiry,
//                                    ulong toSelfDelay, ulong amountMilliSats)
//         : base(hasAnchorOutputs, network, output, revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats)
//     {
//         SetLockTime(cltvEpiry);
//     }
//
//     protected new List<ECDSASignature> SignTransaction(ILightningSigner signer, params BitcoinSecret[] secrets)
//     {
//         var witness = new WitScript(
//             Op.GetPushOp(0), // OP_0
//             Op.GetPushOp(0), // Remote signature
//             Op.GetPushOp(0), // Local signature
//             Op.GetPushOp([]) // Payment pre-image for HTLC-success
//         );
//
//         return base.SignTransaction(signer, secrets);
//     }
// }