using NBitcoin;

namespace NLightning.Integration.Tests.Docker.Mock;

using Domain.Protocol.Managers;

public class FakeSecureKeyManager : ISecureKeyManager
{
    private readonly ExtKey _nodeKey;
    private uint _index = 0;

    public FakeSecureKeyManager()
    {
        _nodeKey = new ExtKey(new Key(), Network.RegTest.GenesisHash.ToBytes());
    }

    public ExtKey GetNextKey(out uint index)
    {
        index = _index++;
        return _nodeKey.Derive(new KeyPath($"m/0/{index}"));
    }

    public Key GetNodeKey()
    {
        return _nodeKey.PrivateKey;
    }

    public PubKey GetNodePubKey()
    {
        return _nodeKey.PrivateKey.PubKey;
    }

    public void SaveToFile(string filePath, string password)
    {
        throw new NotImplementedException();
    }
}