namespace NLightning.Bolts.Base;

using BOLT1.Payloads;
using Common.BitUtils;
using Interfaces;

/// <summary>
/// Base class for a message.
/// </summary>
public abstract class BaseMessage : IMessage
{
    /// <inheritdoc />
    public ushort Type { get; protected set; }

    /// <inheritdoc />
    public virtual IMessagePayload Payload { get; protected set; }

    /// <inheritdoc />
    public TlvStream? Extension { get; protected set; }

    protected BaseMessage(ushort type, IMessagePayload payload, TlvStream? extension = null)
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

    /// <inheritdoc />
    /// <exception cref="NullReferenceException">Payload must not be null.</exception>
    public virtual async Task SerializeAsync(Stream stream)
    {
        if (Payload == null)
        {
            throw new NullReferenceException("Payload must not be null.");
        }

        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Type));

        await Payload.SerializeAsync(stream);

        if (Extension?.Any() ?? false)
        {
            foreach (var tlv in Extension.GetTlvs())
            {
                await tlv.SerializeAsync(stream);
            }
        }
    }
}