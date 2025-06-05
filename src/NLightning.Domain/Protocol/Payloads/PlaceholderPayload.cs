using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;

/// <summary>
/// Placeholder payload for messages that will be deserialized later.
/// </summary>
/// <remarks>
/// DON'T USE THIS PAYLOAD IN REAL MESSAGES!
/// </remarks>
internal sealed class PlaceholderPayload : IChannelMessagePayload
{
    public ChannelId ChannelId
    {
        get
        {
            throw new NotImplementedException("This class should be just a placeholder.");
        }
    }
}