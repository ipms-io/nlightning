using System.Diagnostics.CodeAnalysis;
using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Constants;

/// <summary>
/// Constants for TLV.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TlvConstants
{
    /// <summary>
    /// Funding output contribution TLV type.
    /// </summary>
    /// <remarks>
    /// The funding output contribution TLV type is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
    /// </remarks>
    public static readonly BigSize FundingOutputContribution = 0;

    /// <summary>
    /// Networks TLV type.
    /// </summary>
    /// <remarks>
    /// The networks TLV type is used in the InitMessage to communicate the networks that the node supports.
    /// </remarks>
    public static readonly BigSize Networks = 1;

    /// <summary>
    /// Remote address TLV type.
    /// </summary>
    /// <remarks>
    /// The remote address TLV type is used in the InitMessage to communicate the remote address of the node.
    /// </remarks>
    public static readonly BigSize RemoteAddress = 3;

    /// <summary>
    /// Upfront Shutdown Script TLV Type
    /// </summary>
    /// <remarks>
    /// The "Upfront Shutdown Script" is used in the OpenChannel2Message to set where the funds should be sent on a
    /// mutual close
    /// </remarks>
    public static readonly BigSize UpfrontShutdownScript = 0;

    /// <summary>
    /// Channel Type TLV Type
    /// </summary>
    /// <remarks>
    /// The "Channel Type" is used in the OpenChannel2Message to set the channel type being oppened
    /// </remarks>
    public static readonly BigSize ChannelType = 1;

    /// <summary>
    /// Require confirmed inputs TLV type.
    /// </summary>
    /// <remarks>
    /// The "Require Confirmed Inputs" TLV type is used in the TxInitRbfMessage and OpenChannel2Message to communicate
    /// if the node requires confirmed inputs.
    /// </remarks>
    public static readonly BigSize RequireConfirmedInputs = 2;

    /// <summary>
    /// Fee Range TLV Type
    /// </summary>
    /// <remarks>
    /// The "Fee Range" is used in the ClosingSignedMessage to set the acceptable fee range
    /// </remarks>
    public static readonly BigSize FeeRange = 1;

    /// <summary>
    /// Short Channel ID TLV Type
    /// </summary>
    /// <remarks>
    /// The "Short Channel ID" is used in the ChannelReadyMessage
    /// </remarks>
    public static readonly BigSize ShortChannelId = 1;

    /// <summary>
    /// Blinded Path TLV Type
    /// </summary>
    /// <remarks>
    /// The "Blinded Path" is used in the UpdateAddHtlcMessage
    /// </remarks>
    public static readonly BigSize BlindedPath = 0;

    /// <summary>
    /// Next Funding TLV Type
    /// </summary>
    /// <remarks>
    /// The "Next Funding" is used in the ChannelReestablishMessage
    /// </remarks>
    public static readonly BigSize NextFunding = 0;
}