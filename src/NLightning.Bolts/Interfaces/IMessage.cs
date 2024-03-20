namespace NLightning.Bolts.Interfaces;

using BOLT1.Types;

public interface IMessage<PayloadType> where PayloadType : IMessagePayload
{
    ushort MessageType { get; }
    PayloadType? Data { get; }
    TLVStream? Extension { get; set; }

    byte[] Serialize();
    void Deserialize(byte[] data);
}