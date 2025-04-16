using NLightning.Common.Interfaces;
using NLightning.Common.Messages;

namespace NLightning.Bolts.Tests.Base.Mock;

using Common.Types;

public class FakeMessage(ushort type, IMessagePayload payload, TlvStream? extension = null) : BaseMessage(type, payload, extension)
{
}