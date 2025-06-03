// using NBitcoin;
// using NBitcoin.Crypto;
//
// namespace NLightning.Infrastructure.Bitcoin.Transactions;
//
// using Domain.Protocol.Signers;
// using Outputs;
//
// public class BaseHtlcSuccessTransaction : BaseHtlcTransaction
// {
//     public byte[] PaymentPreimage { get; }
//
//     public BaseHtlcSuccessTransaction(Network network, bool hasAnchorOutputs, BaseHtlcOutput output,
//                                   PubKey revocationPubKey, PubKey localDelayedPubKey, ulong toSelfDelay,
//                                   ulong amountMilliSats, byte[] paymentPreimage)
//         : base(hasAnchorOutputs, network, output, revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats)
//     {
//         SetLockTime(0);
//         PaymentPreimage = paymentPreimage;
//     }
//
//     protected new List<ECDSASignature> SignTransaction(ILightningSigner signer, params BitcoinSecret[] secrets)
//     {
//         var witness = new WitScript(
//             Op.GetPushOp(0), // OP_0
//             Op.GetPushOp(0), // Remote signature
//             Op.GetPushOp(0), // Local signature
//             Op.GetPushOp(PaymentPreimage) // Payment pre-image for HTLC-success
//         );
//
//         return base.SignTransaction(signer, secrets);
//     }
// }