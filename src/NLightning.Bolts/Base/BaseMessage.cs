namespace NLightning.Bolts.Base;

using Interfaces;
using NLightning.Bolts.BOLT1.Payloads;

public abstract class BaseMessage : IMessage
{
    public ushort Type { get; protected set; }

    public virtual IMessagePayload Payload { get; protected set; }

    public TLVStream? Extension { get; protected set; }

    protected BaseMessage(ushort type, IMessagePayload payload, TLVStream? extension = null)
    {
        Type = type;
        Payload = payload;
        Extension = extension;
    }
    protected internal BaseMessage(ushort type)
    {
        Type = type;
        Payload = new PlaceholderPayload();
    }

    public virtual async Task SerializeAsync(Stream stream)
    {
        if (Payload == null)
        {
            throw new NullReferenceException("Payload must not be null.");
        }

        await stream.WriteAsync(EndianBitConverter.GetBytesBE(Type));

        await Payload.SerializeAsync(stream);

        if (Extension?.Any() ?? false)
        {
            foreach (var tlv in Extension.Ordered())
            {
                await stream.WriteAsync(tlv.Serialize());
            }
        }
    }
}