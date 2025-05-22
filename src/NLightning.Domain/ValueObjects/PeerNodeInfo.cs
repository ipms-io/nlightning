namespace NLightning.Domain.ValueObjects;

using Interfaces;

public readonly record struct PeerNodeInfo(string Address) : IValueObject;