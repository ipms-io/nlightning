using NBitcoin;

namespace NLightning.Domain.Protocol.Managers;

public interface ISecureKeyManager
{
    ExtKey GetNextKey(out uint index);
    ExtKey GetKeyAtIndex(uint index);
    Key GetNodeKey();
    PubKey GetNodePubKey();
}