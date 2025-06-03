using NLightning.Domain.Interfaces;

namespace NLightning.Domain.Node.ValueObjects;

public readonly record struct PeerNodeInfo(string Address) : IValueObject;