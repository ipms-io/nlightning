namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Constants;
using Payloads;

public sealed class PingMessage() : BaseMessage(MessageTypes.PING, new PingPayload())
{
    public new PingPayload Payload
    {
        get => (PingPayload)base.Payload;
        private set => base.Payload = value;
    }

    public static async Task<PingMessage> DeserializeAsync(Stream stream)
    {
        var payload = await PingPayload.DeserializeAsync(stream);

        return new PingMessage
        {
            Payload = payload
        };
    }
}