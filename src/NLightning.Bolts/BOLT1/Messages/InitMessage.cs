namespace NLightning.Bolts.BOLT1.Messages;

using Base;
using Common.Constants;
using Types;

public sealed class InitMessage : BaseMessage<InitPayload>
{
    public override ushort MessageType => 16;

    public override InitPayload? Payload { get; set; }

    public override TLVStream? Extension { get; set; } = new();

    public override Func<BinaryReader, InitPayload> PayloadFactory => InitPayload.Deserialize;

    public InitMessage(byte[] globalFeatures, byte[] localFeatures)
    {
        Payload = new(globalFeatures, localFeatures);
        Extension = new();

        // using main for now
        Extension.Add(new(new BigSize(1), ChainConstants.Main));
    }
    public InitMessage()
    { }

    public static InitMessage FromReader(BinaryReader reader)
    {
        return FromReader<InitMessage>(reader);
    }
}