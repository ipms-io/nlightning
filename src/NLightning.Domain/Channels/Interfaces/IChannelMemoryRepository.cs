using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Channels.Interfaces;

using Crypto.ValueObjects;
using Enums;
using Models;
using ValueObjects;

public interface IChannelMemoryRepository
{
    bool TryGetChannel(ChannelId channelId, [MaybeNullWhen(false)] out ChannelModel channel);

    List<ChannelModel> FindChannels(Func<ChannelModel, bool> predicate);

    bool TryGetChannelState(ChannelId channelId, out ChannelState channelState);
    void AddChannel(ChannelModel channel);
    void UpdateChannel(ChannelModel channel);
    void RemoveChannel(ChannelId channelId);

    bool TryGetTemporaryChannel(CompactPubKey compactPubKey, ChannelId channelId,
                                [MaybeNullWhen(false)] out ChannelModel channel);

    bool TryGetTemporaryChannelState(CompactPubKey compactPubKey, ChannelId channelId, out ChannelState channelState);
    void AddTemporaryChannel(CompactPubKey compactPubKey, ChannelModel channel);
    void UpdateTemporaryChannel(CompactPubKey compactPubKey, ChannelModel channel);
    void RemoveTemporaryChannel(CompactPubKey compactPubKey, ChannelId channelId);
}