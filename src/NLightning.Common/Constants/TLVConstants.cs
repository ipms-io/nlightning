using NLightning.Common.Types;

namespace NLightning.Common.Constants;

/// <summary>
/// Constants for TLV.
/// </summary>
public static class TlvConstants
{
    /// <summary>
    /// Funding output contribution TLV type.
    /// </summary>
    /// <remarks>
    /// The funding output contribution TLV type is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
    /// </remarks>
    public static readonly BigSize FUNDING_OUTPUT_CONTRIBUTION = 0;

    /// <summary>
    /// Networks TLV type.
    /// </summary>
    /// <remarks>
    /// The networks TLV type is used in the InitMessage to communicate the networks that the node supports.
    /// </remarks>
    public static readonly BigSize NETWORKS = 1;

    /// <summary>
    /// Require confirmed inputs TLV type.
    /// </summary>
    /// <remarks>
    /// The require confirmed inputs TLV type is used in the TxInitRbfMessage to communicate if the node requires confirmed inputs.
    /// </remarks>
    public static readonly BigSize REQUIRE_CONFIRMED_INPUTS = 2;

    /// <summary>
    /// Remote address TLV type.
    /// </summary>
    /// <remarks>
    /// The remote address TLV type is used in the InitMessage to communicate the remote address of the node.
    /// </remarks>
    public static readonly BigSize REMOTE_ADDRESS = 3;
}