namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.Interfaces;

public static class Helpers
{
    public static async Task<Stream> CreateStreamFromPayloadAsync(IMessagePayload payload)
    {
        var stream = new MemoryStream();
        await payload.SerializeAsync(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}