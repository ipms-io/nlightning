namespace NLightning.Bolts.BOLT1.Messages;

using Base;
using Types;

public sealed class InitMessage(byte[] globalFeatures, byte[] localFeatures) : BaseMessage<InitData>
{
    public override ushort MessageType => 16;

    public override InitData? Data => throw new NotImplementedException();
}
