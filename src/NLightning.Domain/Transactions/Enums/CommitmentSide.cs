namespace NLightning.Domain.Transactions.Enums;

/// <summary>
/// Specifies which side of the channel's commitment to create.
/// </summary>
public enum CommitmentSide : byte
{
    /// <summary>
    /// The local node's commitment transaction.
    /// </summary>
    Local = 0,
    
    /// <summary>
    /// The remote node's commitment transaction.
    /// </summary>
    Remote = 1,
}