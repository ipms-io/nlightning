using System.Runtime.Serialization;
using NLightning.Domain.Node;
using NLightning.Domain.Serialization.Payloads;

namespace NLightning.Infrastructure.Serialization.Payloads;

using Domain.Protocol.Payloads;
using Domain.Protocol.Payloads.Interfaces;
using Exceptions;
using Interfaces;

public class InitPayloadSerializer : IPayloadSerializer<InitPayload>
{
    private readonly IFeatureSetSerializer _featureSetSerializer;

    public InitPayloadSerializer(IFeatureSetSerializer featureSetSerializer)
    {
        _featureSetSerializer = featureSetSerializer;
    }
    
    public async Task SerializeAsync(IMessagePayload payload, Stream stream)
    {
        if (payload is not InitPayload initPayload)
            throw new SerializationException($"Payload is not of type {nameof(InitPayload)}");
        
        await _featureSetSerializer.SerializeAsync(initPayload.FeatureSet, stream, true);
        await _featureSetSerializer.SerializeAsync(initPayload.FeatureSet, stream);
    }

    public async Task<InitPayload?> DeserializeAsync(Stream stream)
    {
        try
        {
            var globalFeatures = await _featureSetSerializer.DeserializeAsync(stream);
            var features = await _featureSetSerializer.DeserializeAsync(stream);

            return new InitPayload(FeatureSet.Combine(globalFeatures, features));
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing InitPayload", e);
        }
    }

    async Task<IMessagePayload?> IPayloadSerializer.DeserializeAsync(Stream stream)
    {
        return await DeserializeAsync(stream);
    }
}