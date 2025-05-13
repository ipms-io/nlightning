using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Domain.Protocol.Payloads;

/// <summary>
/// Placeholder payload for messages that will be deserialized later.
/// </summary>
/// <remarks>
/// DON'T USE THIS PAYLOAD IN REAL MESSAGES!
/// </remarks>
internal sealed class PlaceholderPayload : IMessagePayload
{
    public Task SerializeAsync(Stream stream)
    {
        throw new NotImplementedException("This class should be just a placeholder.");
    }
}