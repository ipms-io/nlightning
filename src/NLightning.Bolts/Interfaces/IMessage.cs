namespace NLightning.Bolts.Interfaces;

using BOLT1.Types;

public interface IMessage<PayloadType> where PayloadType : IMessagePayload
{
    ushort MessageType { get; }
    PayloadType? Payload { get; }
    TLVStream? Extension { get; set; }
    Func<BinaryReader, PayloadType> PayloadFactory { get; }

    void ToWriter(BinaryWriter writer);
    byte[] Serialize();
}