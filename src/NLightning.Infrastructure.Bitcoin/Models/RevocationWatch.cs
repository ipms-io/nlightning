using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Models;

public class RevocationWatch
{
    public uint256 CommitmentTxId { get; }
    public Transaction PenaltyTransaction { get; }

    public RevocationWatch(uint256 commitmentTxId, Transaction penaltyTx)
    {
        CommitmentTxId = commitmentTxId;
        PenaltyTransaction = penaltyTx;
    }
}