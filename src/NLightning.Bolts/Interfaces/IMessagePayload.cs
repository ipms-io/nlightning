namespace NLightning.Bolts.Interfaces;

public interface IMessagePayload
{
    void ToWriter(BinaryWriter writer);
}