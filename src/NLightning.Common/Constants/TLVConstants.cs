using NLightning.Common.Types;

namespace NLightning.Common.Constants;

/// <summary>
/// Constants for TLV.
/// </summary>
public static class TlvConstants
{
    /// <summary>
    /// Networks TLV type.
    /// </summary>
    /// <remarks>
    /// The networks TLV type is used in the InitMessage to communicate the networks that the node supports.
    /// </remarks>
    public static readonly BigSize NETWORKS = 1;

    /// <summary>
    /// Remote address TLV type.
    /// </summary>
    /// <remarks>
    /// The remote address TLV type is used in the InitMessage to communicate the remote address of the node.
    /// </remarks>
    public static readonly BigSize REMOTE_ADDRESS = 3;
}