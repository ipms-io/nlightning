namespace NLightning.Bolts.BOLT1.Types;

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
    public InitPayload(BinaryReader reader)
    {
        var globalFeatures = new Features();
        globalFeatures.Deserialize(reader);

        var features = new Features();
        features.Deserialize(reader);

        Features = Features.Combine(globalFeatures, features);
    }

    public void Serialize(BinaryWriter writer)
    {
        Features.Serialize(writer, true);
        Features.Serialize(writer);
    }
}