namespace NLightning.Bolts.Interfaces;

public interface IMessagePayload
{
    void Serialize(BinaryWriter writer);
}