using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Protocol.Interfaces;

public interface ISecureKeyManager
{
    BitcoinKeyPath KeyPath { get; }

    ExtPrivKey GetNextKey(out uint index);
    ExtPrivKey GetKeyAtIndex(uint index);
    CryptoKeyPair GetNodeKeyPair();
    CompactPubKey GetNodePubKey();
}