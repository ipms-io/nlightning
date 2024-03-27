namespace NLightning.Bolts.Interfaces;

public interface IMessagePayload
{
    Task SerializeAsync(Stream stream);
}