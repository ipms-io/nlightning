namespace NLightning.Bolts.Messages;

using Interfaces;

public sealed class Message : IMessage
{
    public ushort Type { get; private set; }

    public IMessagePayload Payload { get; private set; }

    public TLVStream? Extension { get; private set; } = new();

    public Message(ushort type, IMessagePayload payload, TLVStream? extension = null)
    {
        Type = type;
        Payload = payload;

        // Add any additional TLVs to extension
        if (extension != null)
        {
            foreach (var tlv in extension.GetTlvs())
            {
                Extension.Add(tlv);
            }
        }
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(EndianBitConverter.GetBytesBE(Type));
        Payload?.Serialize(writer);

        if (Extension?.Any() ?? false)
        {
            Extension.Ordered() // Order by type
                     .ToList()  // Convert to list
                     .ForEach(tlv => writer.Write(tlv.Serialize()));
        }
    }
}