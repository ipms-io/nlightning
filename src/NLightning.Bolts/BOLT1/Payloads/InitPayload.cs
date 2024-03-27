namespace NLightning.Bolts.BOLT1.Payloads;

using System.IO;
using Bolts.BOLT9;
using Bolts.Interfaces;

public class InitPayload : IMessagePayload
{
    public Features Features { get; }

    public InitPayload(Features features)
    {
        Features = features;
    }

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