namespace NLightning.Bolts.Interfaces;

public interface IMessagePayload
{
    byte[] Serialize();
    void Deserialize(BinaryReader data);
}