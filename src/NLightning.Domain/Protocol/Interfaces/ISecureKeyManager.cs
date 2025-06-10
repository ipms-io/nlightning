namespace NLightning.Domain.Protocol.Interfaces;

using Bitcoin.ValueObjects;
using Crypto.ValueObjects;

public interface ISecureKeyManager
{
    BitcoinKeyPath KeyPath { get; }

    ExtPrivKey GetNextKey(out uint index);
    ExtPrivKey GetKeyAtIndex(uint index);
    CryptoKeyPair GetNodeKeyPair();
    CompactPubKey GetNodePubKey();
}