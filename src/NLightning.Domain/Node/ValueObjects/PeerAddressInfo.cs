namespace NLightning.Domain.Node.ValueObjects;

using Domain.Interfaces;

public readonly record struct PeerAddressInfo(string Address) : IValueObject;