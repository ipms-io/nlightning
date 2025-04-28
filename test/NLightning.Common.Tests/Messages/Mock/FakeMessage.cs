namespace NLightning.Common.Tests.Messages.Mock;

using Common.Messages;
using Common.Types;
using Interfaces;

public class FakeMessage(ushort type, IMessagePayload payload, TlvStream? extension = null) : BaseMessage(type, payload, extension)
{
}