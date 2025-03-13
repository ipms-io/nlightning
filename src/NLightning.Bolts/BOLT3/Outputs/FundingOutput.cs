using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

public class FundingOutput : OutputBase
{
    public PubKey PubKey1 { get; }
    public PubKey PubKey2 { get; }
    public Script RedeemScript { get; }

    public FundingOutput(PubKey pubkey1, PubKey pubkey2, ulong amountSats)
        : base(GenerateFundingScript(pubkey1, pubkey2), amountSats)
    {
        ArgumentNullException.ThrowIfNull(pubkey1);
        ArgumentNullException.ThrowIfNull(pubkey2);

        if (pubkey1 == pubkey2)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats == 0)
            throw new ArgumentException("Funding amount must be greater than zero.");

        var orderedKeys = new[] { pubkey1, pubkey2 }.OrderBy(pk => pk, PubKeyComparer.Instance).ToArray();
        PubKey1 = orderedKeys[0];
        PubKey2 = orderedKeys[1];
        RedeemScript = CreateMultisigScript(PubKey1, PubKey2);
    }

    private static Script GenerateFundingScript(PubKey pubkey1, PubKey pubkey2)
    {
        var multisigScript = CreateMultisigScript(pubkey1, pubkey2);

        // Convert to P2WSH as required by BOLT3
        return multisigScript.WitHash.ScriptPubKey;
    }

    private static Script CreateMultisigScript(PubKey pubkey1, PubKey pubkey2)
    {
        var orderedKeys = new[] { pubkey1, pubkey2 }.OrderBy(pk => pk, PubKeyComparer.Instance).ToArray();
        return PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, orderedKeys);
    }
}