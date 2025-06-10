namespace NLightning.Domain.Node.ValueObjects;

using Interfaces;

public readonly record struct PeerNodeInfo(string Address) : IValueObject;