namespace NLightning.Bolts.BOLT1.Messages;

using Bolts.Base;
using Constants;
using Payloads;

public sealed class ErrorMessage(ErrorPayload payload) : BaseMessage(MessageTypes.ERROR, payload)
{
    public new ErrorPayload Payload { get => (ErrorPayload)base.Payload; }

    public static async Task<ErrorMessage> DeserializeAsync(Stream stream)
    {
        // Deserialize payload
        var payload = await ErrorPayload.DeserializeAsync(stream);

        return new ErrorMessage(payload);
    }
}