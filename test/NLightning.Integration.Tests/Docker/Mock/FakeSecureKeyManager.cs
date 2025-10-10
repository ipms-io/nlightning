using NBitcoin;

namespace NLightning.Integration.Tests.Docker.Mock;

using Domain.Bitcoin.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.Interfaces;

public class FakeSecureKeyManager : ISecureKeyManager
{
    private readonly ExtKey _nodeKey;

    public BitcoinKeyPath KeyPath => new BitcoinKeyPath([]);
    public uint HeightOfBirth { get; }

    public FakeSecureKeyManager()
    {
        _nodeKey = new ExtKey(new Key(), Network.RegTest.GenesisHash.ToBytes());
    }

    public ExtPrivKey GetNextKey(out uint index)
    {
        index = 0;
        return _nodeKey.ToBytes();
    }

    public ExtPrivKey GetKeyAtIndex(uint index)
    {
        return _nodeKey.ToBytes();
    }

    public CryptoKeyPair GetNodeKeyPair()
    {
        return new CryptoKeyPair(_nodeKey.PrivateKey.ToBytes(), _nodeKey.PrivateKey.PubKey.ToBytes());
    }

    public CompactPubKey GetNodePubKey()
    {
        return _nodeKey.PrivateKey.PubKey.ToBytes();
    }
}