namespace NLightning.Domain.Transactions.Enums;

/// <summary>
/// Enumerates the different types of outputs in a Lightning commitment transaction.
/// </summary>
public enum OutputType : byte
{
    ToLocal = 1,
    ToRemote = 2,
    LocalAnchor = 3,
    RemoteAnchor = 4,
    OfferedHtlc = 5,
    ReceivedHtlc = 6,
    Funding = 7
}