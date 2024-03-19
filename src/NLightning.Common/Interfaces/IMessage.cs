namespace NLightning.Common.Interfaces;

public interface IMessage
{
    byte MessageType { get; }

    byte[] Serialize();
    void Deserialize(byte[] data);
}