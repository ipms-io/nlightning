using NLightning.Common.Interfaces;

namespace NLightning.Bolts.Tests.Base.Mock;

using Bolts.Base;
using Common.Types;

public class FakeMessage(ushort type, IMessagePayload payload, TlvStream? extension = null) : BaseMessage(type, payload, extension)
{
}