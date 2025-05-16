using NLightning.Domain.Protocol.Factories;

namespace NLightning.Infrastructure.Protocol.Factories;

public class HtlcTransactionFactory : IHtlcTransactionFactory
{
    // public HtlcTimeoutTransaction CreateHtlcTimeoutTransaction(OutPoint outPoint, PubKey revocationPubKey,
    //                                                            PubKey localDelayedPubKey, uint cltvExpiry,
    //                                                            ulong toSelfDelay, ulong amountMilliSats, ulong feesSats)
    // {
    //     return new HtlcTimeoutTransaction(outPoint, revocationPubKey, localDelayedPubKey, cltvExpiry, toSelfDelay,
    //                                       amountMilliSats, feesSats);
    // }

    // public HtlcSuccessTransaction CreateHtlcSuccessTransaction(OutPoint outPoint, PubKey revocationPubKey,
    //                                                            PubKey localDelayedPubKey, ulong toSelfDelay,
    //                                                            ulong amountMilliSats, ulong feesSats)
    // {
    //     return new HtlcSuccessTransaction(outPoint, revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats,
    //                                       feesSats);
    // }
}