namespace NLightning.Bolts.Interfaces;
public interface IMessage
{
    ushort Type { get; }
    IMessagePayload Payload { get; }
    TLVStream? Extension { get; }
}