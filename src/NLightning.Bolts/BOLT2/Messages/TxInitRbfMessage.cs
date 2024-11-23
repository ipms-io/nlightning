using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Common.Constants;
using Common.TLVs;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_init_rbf message.
/// </summary>
/// <remarks>
/// The tx_init_rbf message initiates a replacement of the transaction after it's been completed.
/// The message type is 72.
/// </remarks>
public sealed class TxInitRbfMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxInitRbfPayload Payload { get => (TxInitRbfPayload)base.Payload; }

    public FundingOutputContributionTlv? FundingOutputContributionTlv { get; }
    public RequireConfirmedInputsTlv? RequireConfirmedInputsTlv { get; }

    public TxInitRbfMessage(TxInitRbfPayload payload, FundingOutputContributionTlv? fundingOutputContributionTlv = null, RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null)
        : base(MessageTypes.TX_INIT_RBF, payload)
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
    /// Deserialize a TxInitRbfMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxInitRbfMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxInitRbfMessage</exception>
    public static async Task<TxInitRbfMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxInitRbfPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new TxInitRbfMessage(payload);
            }

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.FUNDING_OUTPUT_CONTRIBUTION, out var tlv)
                ? FundingOutputContributionTlv.FromTlv(tlv!)
                : null;

            var requireConfirmedInputsTlv = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out tlv)
                ? RequireConfirmedInputsTlv.FromTlv(tlv!)
                : null;

            return new TxInitRbfMessage(payload, channelTypeTlv, requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxInitRbfMessage", e);
        }
    }
}