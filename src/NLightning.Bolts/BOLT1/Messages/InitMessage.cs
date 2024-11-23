using System.Runtime.Serialization;
using NLightning.Common.Constants;

namespace NLightning.Bolts.BOLT1.Messages;

using Base;
using Common.TLVs;
using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents an init message.
/// </summary>
/// <remarks>
/// The init message is used to communicate the features of the node.
/// The message type is 16.
/// </remarks>
public sealed class InitMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new InitPayload Payload { get => (InitPayload)base.Payload; }

    public NetworksTlv? NetworksTlv { get; }

    public InitMessage(InitPayload payload, NetworksTlv? networksTlv = null) : base(MessageTypes.INIT, payload)
    {
        NetworksTlv = networksTlv;

        if (networksTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(networksTlv);
        }
    }

    /// <summary>
    /// Deserialize an InitMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized InitMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing InitMessage</exception>
    public static async Task<InitMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await InitPayload.DeserializeAsync(stream);

            // Deserialize extension if available
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new InitMessage(payload);
            }

            var networksTlv = extension.TryGetTlv(TlvConstants.NETWORKS, out var tlv)
                ? NetworksTlv.FromTlv(tlv!)
                : null;

            return new InitMessage(payload, networksTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing InitMessage", e);
        }
    }
}