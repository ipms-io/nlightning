using NBitcoin;

namespace NLightning.Domain.Protocol.Managers;

public interface ISecureKeyManager
{
    KeyPath KeyPath { get; }

    ExtKey GetNextKey(out uint index);
    ExtKey GetKeyAtIndex(uint index);
    Key GetNodeKey();
    PubKey GetNodePubKey();
}