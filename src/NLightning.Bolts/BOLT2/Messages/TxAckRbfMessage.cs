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

    public FundingOutputContributionTlv? FundingOutputContributionTlv { get; }
    public RequireConfirmedInputsTlv? RequireConfirmedInputsTlv { get; }

    public TxAckRbfMessage(TxAckRbfPayload payload, FundingOutputContributionTlv? fundingOutputContributionTlv = null, RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null)
        : base(MessageTypes.TX_ACK_RBF, payload)
    {
        FundingOutputContributionTlv = fundingOutputContributionTlv;
        RequireConfirmedInputsTlv = requireConfirmedInputsTlv;

        if (FundingOutputContributionTlv is not null || RequireConfirmedInputsTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(FundingOutputContributionTlv, RequireConfirmedInputsTlv);
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

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION, out var tlv)
                ? FundingOutputContributionTlv.FromTlv(tlv!)
                : null;

            var requireConfirmedInputsTlv = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out tlv)
                ? RequireConfirmedInputsTlv.FromTlv(tlv!)
                : null;

            return new TxAckRbfMessage(payload, channelTypeTlv, requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAckRbfMessage", e);
        }
    }
}