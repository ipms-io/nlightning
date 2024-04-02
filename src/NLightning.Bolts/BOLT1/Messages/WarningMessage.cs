namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Constants;
using Payloads;

public sealed class WarningMessage(ErrorPayload payload) : BaseMessage(MessageTypes.WARNING, payload)
{
    public new ErrorPayload Payload { get => (ErrorPayload)base.Payload; }

    public static async Task<WarningMessage> DeserializeAsync(Stream stream)
    {
        // Deserialize payload
        var payload = await ErrorPayload.DeserializeAsync(stream);

        return new WarningMessage(payload);
    }
}