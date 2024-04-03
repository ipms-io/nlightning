namespace NLightning.Bolts.Tests.Base.Mock;

using Bolts.Base;
using Bolts.Interfaces;
using Common.Types;

public class FakeMessage(ushort type, IMessagePayload payload, TLVStream? extension = null) : BaseMessage(type, payload, extension)
{
}