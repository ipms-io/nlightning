// using NBitcoin;
// using NLightning.Bolts.BOLT3.Calculators;
//
// namespace NLightning.Bolts.BOLT3.Transactions;
//
// using Common.Managers;
// using Comparers;
// using Constants;
// using Outputs;
// using Types;
//
// /// <summary>
// /// Represents a commitment transaction.
// /// </summary>
// public class CommitmentTransaction : BaseTransaction
// {
//     private readonly bool _isChannelFunder;
//     private readonly IList<OutputBase> _outputList = [];
//     private readonly IList<OfferedHtlcOutput> _offeredHtlcOutputs = [];
//     private readonly IList<ReceivedHtlcOutput> _receivedHtlcOutputs = [];
//
//     public PubKey LocalPubKey { get; }
//     public ToLocalOutput ToLocalOutput => _outputList[0] as ToLocalOutput ?? throw new InvalidCastException("Output 0 was not type ToLocalOutput");
//     public ToRemoteOutput ToRemoteOutput => _outputList[1] as ToRemoteOutput ?? throw new InvalidCastException("Output 1 was not type ToRemoteOutput");
//     public ToAnchorOutput? LocalAnchorOutput => ConfigManager.Instance.IsOptionAnchorOutput
//         ? _outputList[2] as ToAnchorOutput ?? throw new InvalidCastException("Output 2 was not type ToAnchorOutput")
//         : null;
//     public ToAnchorOutput? RemoteAnchorOutput => ConfigManager.Instance.IsOptionAnchorOutput
//         ? _outputList[3] as ToAnchorOutput ?? throw new InvalidCastException("Output 3 was not type ToAnchorOutput")
//         : null;
//
//     public CommitmentNumber CommitmentNumber { get; }
//
//     /// <summary>
//     /// Initializes a new instance of the <see cref="CommitmentTransaction"/> class.
//     /// </summary>
//     /// <param name="fundingCoin">The funding coin.</param>
//     /// <param name="localPubKey">The local public key.</param>
//     /// <param name="remotePubKey">The remote public key.</param>
//     /// <param name="localDelayedPubKey">The local delayed public key.</param>
//     /// <param name="revocationPubKey">The revocation public key.</param>
//     /// <param name="toLocalAmount">The amount for the to_local output in satoshis.</param>
//     /// <param name="toRemoteAmount">The amount for the to_remote output in satoshis.</param>
//     /// <param name="toSelfDelay">The to_self_delay in blocks.</param>
//     /// <param name="commitmentNumber">The commitment number object.</param>
//     /// <param name="isChannelFunder">Indicates if the local node is the channel funder.</param>
//     internal CommitmentTransaction(
//         Coin fundingCoin,
//         PubKey localPubKey,
//         PubKey remotePubKey,
//         PubKey localDelayedPubKey,
//         PubKey revocationPubKey,
//         Money toLocalAmount,
//         Money toRemoteAmount,
//         uint toSelfDelay,
//         CommitmentNumber commitmentNumber,
//         bool isChannelFunder) : base (TransactionConstants.COMMITMENT_TRANSACTION_VERSION, fundingCoin)
//     {
//         ArgumentNullException.ThrowIfNull(localPubKey);
//         ArgumentNullException.ThrowIfNull(remotePubKey);
//         ArgumentNullException.ThrowIfNull(localDelayedPubKey);
//         ArgumentNullException.ThrowIfNull(revocationPubKey);
//         ArgumentOutOfRangeException.ThrowIfZero(toLocalAmount.Satoshi);
//         ArgumentOutOfRangeException.ThrowIfZero(toRemoteAmount.Satoshi);
//
//         _isChannelFunder = isChannelFunder;
//         LocalPubKey = localPubKey;
//         CommitmentNumber = commitmentNumber;
//         
//         // Set locktime
//         TRANSACTION.LockTime = commitmentNumber.CalculateLockTime();
//
//         // Add the sequence for funding input
//         TRANSACTION.Inputs[0].Sequence = commitmentNumber.CalculateSequence();
//
//         // to_local output
//         if (toLocalAmount >= ConfigManager.Instance.DustLimitAmountMoney) // Dust limit in satoshis
//         {
//             var toLocalOutput = new ToLocalOutput(localDelayedPubKey, revocationPubKey, toSelfDelay, toLocalAmount);
//             _outputList.Add(toLocalOutput);
//             TRANSACTION.Outputs.Add(toLocalOutput.ToTxOut());
//         }
//
//         // to_remote output
//         if (toRemoteAmount >= ConfigManager.Instance.DustLimitAmountMoney) // Dust limit in satoshis
//         {
//             var toRemoteOutput = new ToRemoteOutput(remotePubKey, toRemoteAmount);
//             _outputList.Add(toRemoteOutput);
//             TRANSACTION.Outputs.Add(toRemoteOutput.ToTxOut());
//         }
//
//         if (!ConfigManager.Instance.IsOptionAnchorOutput || ConfigManager.Instance.AnchorAmountSats == 0)
//         {
//             return;
//         }
//
//         // Local anchor output
//         var localAnchor = new ToAnchorOutput(localPubKey, ConfigManager.Instance.AnchorAmountSats);
//         _outputList.Add(localAnchor);
//         TRANSACTION.Outputs.Add(localAnchor.ToTxOut());
//
//         // Remote anchor output
//         var remoteAnchor = new ToAnchorOutput(remotePubKey, ConfigManager.Instance.AnchorAmountSats);
//         _outputList.Add(remoteAnchor);
//         TRANSACTION.Outputs.Add(remoteAnchor.ToTxOut());
//     }
//
//     internal Transaction SignAndFinalizeTransaction(FeeCalculator feeCalculator, params Key[] keys)
//     {
//         var outputAmount = (ulong)_outputList.Sum(o => o.AmountSats ?? 0UL);
//         
//         // Check if output amount is greater than input amount
//         if (outputAmount >= (Money)_fundingCoin.Amount)
//             throw new InvalidOperationException("Output amount cannot exceed input amount + fees.");
//         
//         // Sign all inputs
//         _transaction.Sign(keys.Select(k => new BitcoinSecret(k, ConfigManager.Instance.Network)), [_fundingCoin]);
//     }
//
//     public void AddOfferedHtlcOutputAndUpdate(OfferedHtlcOutput offeredHtlcOutput)
//     {
//         // Add output to lists
//         _offeredHtlcOutputs.Add(offeredHtlcOutput);
//         _outputList.Add(offeredHtlcOutput);
//
//         // Clear TxOuts
//         _transaction.Outputs.Clear();
//
//         // Add ordered outputs
//         _transaction.Outputs.AddRange(
//             _outputList.OrderBy(htlc => htlc, TransactionOutputComparer.Instance)
//                        .Select(htlc => htlc.ToTxOut())
//         );
//     }
//
//     public void AddReceivedHtlcOutputAndUpdate(ReceivedHtlcOutput receivedHtlcOutput)
//     {
//         _receivedHtlcOutputs.Add(receivedHtlcOutput);
//         _outputList.Add(receivedHtlcOutput);
//
//         // Clear TxOuts
//         _transaction.Outputs.Clear();
//
//         // Add ordered outputs
//         _transaction.Outputs.AddRange(
//             _outputList.OrderBy(htlc => htlc, TransactionOutputComparer.Instance)
//                 .Select(htlc => htlc.ToTxOut())
//         );
//     }
// }