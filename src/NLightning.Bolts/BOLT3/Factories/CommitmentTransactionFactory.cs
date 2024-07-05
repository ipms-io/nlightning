using NBitcoin;

namespace NLightning.Bolts.BOLT3.Factories;

using Transactions;

public class CommitmentTransactionFactory
{
    public CommitmentTransaction CreateCommitmentTransaction(
        uint256 fundingTxId,
        uint fundingOutputIndex,
        Money fundingAmount,
        PubKey localPubKey,
        PubKey remotePubKey,
        PubKey localDelayedPubKey,
        PubKey revocationPubKey,
        Money toLocalAmount,
        Money toRemoteAmount,
        uint toSelfDelay,
        ulong obscuredCommitmentNumber,
        List<Htlc> htlcs,
        Money dustLimitSatoshis,
        FeeRate feeRatePerKw,
        bool optionAnchors)
    {
        var tx = Transaction.Create(Network.Main);

        // Initialize the commitment transaction input and locktime
        InitializeTransaction(tx, fundingTxId, fundingOutputIndex, obscuredCommitmentNumber);

        // Calculate which committed HTLCs need to be trimmed
        var trimmedHtlcs = TrimmedOutputs(htlcs, dustLimitSatoshis, feeRatePerKw);

        // Calculate the base commitment transaction fee
        var baseFee = CalculateBaseFee(feeRatePerKw, trimmedHtlcs.Count);

        // Adjust amounts for fees and anchors
        AdjustAmountsForFees(ref toLocalAmount, ref toRemoteAmount, baseFee, optionAnchors);

        // Add HTLC outputs
        AddHtlcOutputs(tx, trimmedHtlcs, localPubKey, remotePubKey);

        // Add to_local output
        if (toLocalAmount >= dustLimitSatoshis)
        {
            AddToLocalOutput(tx, toLocalAmount, localDelayedPubKey, revocationPubKey, toSelfDelay);
        }

        // Add to_remote output
        if (toRemoteAmount >= dustLimitSatoshis)
        {
            AddToRemoteOutput(tx, toRemoteAmount, remotePubKey, optionAnchors);
        }

        // Add anchor outputs if option_anchors applies
        if (optionAnchors)
        {
            AddAnchorOutputs(tx, toLocalAmount, toRemoteAmount, trimmedHtlcs.Count, localPubKey, remotePubKey);
        }

        // Sort the outputs into BIP 69+CLTV order
        SortOutputs(tx);

        return new CommitmentTransaction(tx);
    }

    private void InitializeTransaction(Transaction tx, uint256 fundingTxId, uint fundingOutputIndex, ulong obscuredCommitmentNumber)
    {
        tx.Version = 2;
        tx.LockTime = new LockTime((0x20 << 24) | (uint)(obscuredCommitmentNumber & 0xFFFFFF));
        tx.Inputs.Add(new TxIn(new OutPoint(fundingTxId, fundingOutputIndex))
        {
            Sequence = (0x80 << 24) | (uint)((obscuredCommitmentNumber >> 24) & 0xFFFFFF)
        });
    }

    private List<Htlc> TrimmedOutputs(List<Htlc> htlcs, Money dustLimitSatoshis, FeeRate feeRatePerKw)
    {
        // Implement logic to calculate which HTLCs need to be trimmed
        // based on dust limit and fee rate.
        return htlcs; // Placeholder for actual implementation
    }

    private Money CalculateBaseFee(FeeRate feeRatePerKw, int numHtlcs)
    {
        // Implement logic to calculate the base fee for the transaction.
        return Money.Satoshis(1000); // Placeholder for actual implementation
    }

    private void AdjustAmountsForFees(ref Money toLocalAmount, ref Money toRemoteAmount, Money baseFee, bool optionAnchors)
    {
        var totalFee = baseFee;
        if (optionAnchors)
        {
            totalFee += Money.Satoshis(660); // Two anchors of 330 sat each
        }

        if (toLocalAmount >= toRemoteAmount)
        {
            toLocalAmount -= totalFee;
        }
        else
        {
            toRemoteAmount -= totalFee;
        }
    }

    private void AddHtlcOutputs(Transaction tx, List<Htlc> htlcs, PubKey localPubKey, PubKey remotePubKey)
    {
        // Implement logic to add HTLC outputs to the transaction
        // based on whether they are offered or received.
    }

    private void AddToLocalOutput(Transaction tx, Money toLocalAmount, PubKey localDelayedPubKey, PubKey revocationPubKey, uint toSelfDelay)
    {
        var toLocalScript = CreateToLocalScript(localDelayedPubKey, revocationPubKey, toSelfDelay);
        tx.Outputs.Add(new TxOut(toLocalAmount, toLocalScript.WitHash));
    }

    private void AddToRemoteOutput(Transaction tx, Money toRemoteAmount, PubKey remotePubKey, bool optionAnchors)
    {
        Script toRemoteScript;
        if (optionAnchors)
        {
            toRemoteScript = new Script(
                Op.GetPushOp(remotePubKey.ToBytes()),
                OpcodeType.OP_CHECKSIG,
                OpcodeType.OP_1,
                OpcodeType.OP_CHECKSEQUENCEVERIFY
            );
        }
        else
        {
            toRemoteScript = remotePubKey.Hash.ScriptPubKey;
        }
        tx.Outputs.Add(new TxOut(toRemoteAmount, toRemoteScript.WitHash));
    }

    private void AddAnchorOutputs(Transaction tx, Money toLocalAmount, Money toRemoteAmount, int numHtlcs, PubKey localPubKey, PubKey remotePubKey)
    {
        var anchorAmount = Money.Satoshis(330);

        if (toLocalAmount >= Money.Zero || numHtlcs > 0)
        {
            tx.Outputs.Add(new TxOut(anchorAmount, localPubKey.ScriptPubKey));
        }

        if (toRemoteAmount >= Money.Zero || numHtlcs > 0)
        {
            tx.Outputs.Add(new TxOut(anchorAmount, remotePubKey.ScriptPubKey));
        }
    }

    private void SortOutputs(Transaction tx)
    {
        tx.Outputs = new TxOutList(tx.Outputs.OrderBy(o => o.Value)
                                              .ThenBy(o => o.ScriptPubKey.ToHex())
                                              .ThenBy(o => o.ScriptPubKey.IsUnspendable ? 0 : 1)
                                              .ToList());
    }

    private Script CreateToLocalScript(PubKey localDelayedPubKey, PubKey revocationPubKey, uint toSelfDelay)
    {
        return new Script(
            OpcodeType.OP_IF,
            Op.GetPushOp(revocationPubKey.ToBytes()),
            OpcodeType.OP_ELSE,
            Op.GetPushOp(toSelfDelay),
            OpcodeType.OP_CHECKSEQUENCEVERIFY,
            OpcodeType.OP_DROP,
            Op.GetPushOp(localDelayedPubKey.ToBytes()),
            OpcodeType.OP_ENDIF,
            OpcodeType.OP_CHECKSIG
        );
    }
}