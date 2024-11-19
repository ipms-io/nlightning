using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Common.Constants;
using Common.TLVs;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_ack_rbf message.
/// </summary>
/// <remarks>
/// The tx_ack_rbf message acknowledges the replacement of the transaction.
/// The message type is 73.
/// </remarks>
public sealed class TxAckRbfMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAckRbfPayload Payload { get => (TxAckRbfPayload)base.Payload; }

    public FundingOutputContributionTlv? FundingOutputContribution { get; }
    public RequireConfirmedInputsTlv? RequireConfirmedInputs { get; }

    public TxAckRbfMessage(TxAckRbfPayload payload, FundingOutputContributionTlv? fundingOutputContribution = null, RequireConfirmedInputsTlv? requireConfirmedInputs = null)
        : base(MessageTypes.TX_ACK_RBF, payload)
    {
        FundingOutputContribution = fundingOutputContribution;
        RequireConfirmedInputs = requireConfirmedInputs;

        if (FundingOutputContribution is not null || RequireConfirmedInputs is not null)
        {
            Extension = new TlvStream();
            Extension.Add(FundingOutputContribution, RequireConfirmedInputs);
        }
    }

    /// <summary>
    /// Deserialize a TxAckRbfMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAckRbfMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAckRbfMessage</exception>
    public static async Task<TxAckRbfMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxAckRbfPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new TxAckRbfMessage(payload);
            }

            var channelType = extension.TryGetTlv(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION, out var channelTypeTlv)
                ? FundingOutputContributionTlv.FromTlv(channelTypeTlv!)
                : null;

            var requireConfirmedInputs = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out var requireConfirmedInputsTlv)
                ? RequireConfirmedInputsTlv.FromTlv(requireConfirmedInputsTlv!)
                : null;

            return new TxAckRbfMessage(payload, channelType, requireConfirmedInputs);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAckRbfMessage", e);
        }
    }
}