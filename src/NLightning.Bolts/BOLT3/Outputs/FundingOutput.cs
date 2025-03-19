using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

public class FundingOutput : OutputBase
{
    public PubKey PubKey1 { get; }
    public PubKey PubKey2 { get; }

    public FundingOutput(PubKey pubkey1, PubKey pubkey2, LightningMoney amountSats)
        : base(CreateMultisigScript(pubkey1, pubkey2), amountSats)
    {
        ArgumentNullException.ThrowIfNull(pubkey1);
        ArgumentNullException.ThrowIfNull(pubkey2);

        if (pubkey1 == pubkey2)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats.IsZero)
            throw new ArgumentException("Funding amount must be greater than zero.");

        var orderedKeys = new[] { pubkey1, pubkey2 }.OrderBy(pk => pk, PubKeyComparer.Instance).ToArray();
        PubKey1 = orderedKeys[0];
        PubKey2 = orderedKeys[1];
    }

    private static Script CreateMultisigScript(PubKey pubkey1, PubKey pubkey2)
    {
        var orderedKeys = new[] { pubkey1, pubkey2 }.OrderBy(pk => pk, PubKeyComparer.Instance).ToArray();
        return PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, orderedKeys);
    }
}