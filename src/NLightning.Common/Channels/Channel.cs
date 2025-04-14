using NLightning.Common.Enums;
using NLightning.Common.Types;

namespace NLightning.Common.Channels;

public class Channel
{
    public ChannelId ChannelId { get; }
    public ShortChannelId? ShortChannelId { get; private set; }
    public ChannelState State { get; private set; }
    public bool IsInitiator { get; }

    public Channel(ChannelId channelId, bool isInitiator)
    {
        ChannelId = channelId;
        IsInitiator = isInitiator;
        State = ChannelState.OPENING; // Initial state
    }

    public void AssignShortChannelId(ShortChannelId shortChannelId)
    {
        ShortChannelId = shortChannelId;
        // TODO: Persist the assignment to the database
    }

    public void UpdateState(ChannelState newState)
    {
        // TODO: Persist state change to the database
        State = newState;

        // TODO: Notify other components about the state change
    }
}