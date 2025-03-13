// using NBitcoin;
// using NLightning.Bolts.BOLT3.Enums;
// using NLightning.Bolts.BOLT3.Services;
// using NLightning.Bolts.BOLT3.Transactions;
//
// public class CommitmentTransactionFactory
//     {
//         private readonly FeeCalculator _feeService;
//         private readonly DustService _dustService;
//
//         public CommitmentTransactionFactory(FeeCalculator feeService, DustService dustService)
//         {
//             _feeService = feeService;
//             _dustService = dustService;
//         }
//
//         public Transaction CreateCommitmentTransaction(
//             uint256 txId,
//             int outputIndex,
//             LockTime lockTime,
//             ulong dustLimitSatoshis,
//             ulong feeratePerKw,
//             ulong toLocalAmount,
//             ulong toRemoteAmount,
//             List<Htlc> htlcs,
//             bool optionAnchors)
//         {
//             var transaction = new CommitmentTransaction()
//             {
//                 Version = 2,
//                 LockTime = lockTime
//             };
//
//             transaction.Inputs.Add(new TxIn(new OutPoint(txId, outputIndex)));
//
//             // Calculate the base commitment transaction fee
//             int numUntrimmedHtlcs = htlcs.Count(htlc => !IsHtlcTrimmed(htlc, dustLimitSatoshis, feeratePerKw));
//             ulong baseFee = _feeService.CalculateCommitmentTransactionFee(feeratePerKw, numUntrimmedHtlcs, optionAnchors);
//
//             // Adjust the amounts due to each peer
//             if (toLocalAmount > toRemoteAmount)
//             {
//                 toLocalAmount -= baseFee;
//                 if (optionAnchors)
//                 {
//                     toLocalAmount -= 2 * 330; // Subtract anchor fees
//                 }
//             }
//             else
//             {
//                 toRemoteAmount -= baseFee;
//                 if (optionAnchors)
//                 {
//                     toRemoteAmount -= 2 * 330; // Subtract anchor fees
//                 }
//             }
//
//             // Add HTLC outputs
//             foreach (var htlc in htlcs)
//             {
//                 if (!IsHtlcTrimmed(htlc, dustLimitSatoshis, feeratePerKw))
//                 {
//                     var htlcOutput = CreateHtlcOutput(htlc);
//                     transaction.Outputs.Add(htlcOutput);
//                 }
//             }
//
//             // Add to_local output if necessary
//             if (toLocalAmount >= dustLimitSatoshis)
//             {
//                 var toLocalOutput = new TxOut((Money)toLocalAmount, new Script()); // Use appropriate script
//                 transaction.Outputs.Add(toLocalOutput);
//             }
//
//             // Add to_remote output if necessary
//             if (toRemoteAmount >= dustLimitSatoshis)
//             {
//                 var toRemoteOutput = new TxOut((Money)toRemoteAmount, new Script()); // Use appropriate script
//                 transaction.Outputs.Add(toRemoteOutput);
//             }
//
//             // Add anchor outputs if necessary
//             if (optionAnchors)
//             {
//                 if (toLocalAmount >= dustLimitSatoshis || numUntrimmedHtlcs > 0)
//                 {
//                     var toLocalAnchorOutput = new TxOut((Money)330, new Script()); // Use appropriate script
//                     transaction.Outputs.Add(toLocalAnchorOutput);
//                 }
//                 if (toRemoteAmount >= dustLimitSatoshis || numUntrimmedHtlcs > 0)
//                 {
//                     var toRemoteAnchorOutput = new TxOut((Money)330, new Script()); // Use appropriate script
//                     transaction.Outputs.Add(toRemoteAnchorOutput);
//                 }
//             }
//
//             // Sort outputs into BIP 69+CLTV order (details omitted for brevity)
//             SortOutputs(transaction.Outputs);
//
//             return transaction;
//         }
//
//         private bool IsHtlcTrimmed(Htlc htlc, ulong dustLimitSatoshis, ulong feeratePerKw)
//         {
//             var fee = htlc.Type == HtlcType.OFFERED ? _feeService.CalculateHtlcTimeoutTransactionFee(feeratePerKw, false)
//                                                     : _feeService.CalculateHtlcSuccessTransactionFee(feeratePerKw, false);
//             return htlc.AmountSatoshis < dustLimitSatoshis + fee;
//         }
//
//         private TxOut CreateHtlcOutput(Htlc htlc)
//         {
//             // Create and return an HTLC output based on the htlc details (details omitted for brevity)
//             return new TxOut((Money)htlc.AmountSatoshis, new Script());
//         }
//
//         private void SortOutputs(TxOutList outputs)
//         {
//             // Sort the outputs based on BIP 69+CLTV order (details omitted for brevity)
//         }
//     }
//
//     public class Htlc
//     {
//         public ulong AmountSatoshis { get; set; }
//         public HtlcType Type { get; set; }
//     }