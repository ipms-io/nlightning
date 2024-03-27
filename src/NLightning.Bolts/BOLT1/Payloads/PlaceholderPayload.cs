using NLightning.Bolts.Interfaces;

namespace NLightning.Bolts.BOLT1.Payloads;

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