namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Constants;
using Payloads;

public sealed class InitMessage(InitPayload payload, TLVStream? extension = null) : BaseMessage(MessageTypes.INIT, payload, extension)
{
    public new InitPayload Payload { get => (InitPayload)base.Payload; }

    public static async Task<InitMessage> DeserializeAsync(Stream stream)
    {
        // Deserialize payload
        var payload = await InitPayload.DeserializeAsync(stream);

        // Deserialize extension if available
        var extension = await TLVStream.DeserializeAsync(stream);

        return new InitMessage(payload, extension);
    }
}