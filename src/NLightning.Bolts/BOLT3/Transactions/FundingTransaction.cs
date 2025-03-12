using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

using Outputs;

/// <summary>
/// Represents a funding transaction.
/// </summary>
public class FundingTransaction: Transaction
{
    public FundingOutput FundingOutput { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FundingTransaction"/> class.
    /// </summary>
    /// <param name="pubkey1">The first public key in compressed format.</param>
    /// <param name="pubkey2">The second public key in compressed format.</param>
    /// <param name="amountSats">The amount of the output in satoshis.</param>
    public FundingTransaction(PubKey pubkey1, PubKey pubkey2, ulong amountSats) : base()
    {
        FundingOutput = new FundingOutput(pubkey1, pubkey2, amountSats);
        Outputs.Add(FundingOutput.ToTxOut());
    }
}