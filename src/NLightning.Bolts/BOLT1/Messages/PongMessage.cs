namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Constants;
using Payloads;

public sealed class PongMessage(ushort bytesLen) : BaseMessage(MessageTypes.PONG, new PongPayload(bytesLen))
{
    public new PongPayload Payload
    {
        get => (PongPayload)base.Payload;
        private set => base.Payload = value;
    }

    public static async Task<PongMessage> DeserializeAsync(Stream stream)
    {
        var payload = await PongPayload.DeserializeAsync(stream);

        return new PongMessage(payload.BytesLength)
        {
            Payload = payload
        };
    }
}