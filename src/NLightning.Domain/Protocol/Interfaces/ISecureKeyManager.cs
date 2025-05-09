using NBitcoin;

namespace NLightning.Common.Interfaces;

public interface ISecureKeyManager
{
    ExtKey GetNextKey(out uint index);
    Key GetNodeKey();
    PubKey GetNodePubKey();
}