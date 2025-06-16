using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Bitcoin.Events;

public class NewBlockEventArgs : EventArgs
{
    public uint Height { get; }
    public Hash BlockHash { get; }

    public NewBlockEventArgs(uint height, Hash blockHash)
    {
        Height = height;
        BlockHash = blockHash;
    }
}