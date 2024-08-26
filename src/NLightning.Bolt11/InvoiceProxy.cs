using NBitcoin;

namespace NLightning.Bolt11;

using Common.Types;

public class Invoice : Bolts.BOLT11.Invoice
{
    public Invoice(ulong amountMilliSats, string description, uint256 paymentHash, uint256 paymentSecret) : base(amountMilliSats, description, paymentHash, paymentSecret)
    { }

    public Invoice(ulong amountMilliSats, uint256 descriptionHash, uint256 paymentHash, uint256 paymentSecret) : base(amountMilliSats, descriptionHash, paymentHash, paymentSecret)
    { }

    internal Invoice(Network network, ulong? amountMilliSats, long? timestamp = null) : base(network, amountMilliSats, timestamp)
    { }
}