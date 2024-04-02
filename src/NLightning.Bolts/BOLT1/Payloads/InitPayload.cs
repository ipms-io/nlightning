namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.BOLT9;
using Bolts.Interfaces;

public class InitPayload(Features features) : IMessagePayload
{
    public Features Features { get; } = features;

    public async Task SerializeAsync(Stream stream)
    {
        await Features.SerializeAsync(stream, true);
        await Features.SerializeAsync(stream);
    }

    public static async Task<InitPayload> DeserializeAsync(Stream stream)
    {
        var globalFeatures = await Features.DeserializeAsync(stream, true);

        var features = await Features.DeserializeAsync(stream);

        return new InitPayload(Features.Combine(globalFeatures, features));
    }
}