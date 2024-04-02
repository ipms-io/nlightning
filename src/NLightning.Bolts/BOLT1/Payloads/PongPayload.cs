namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.Interfaces;

public class PongPayload(ushort bytesLen) : IMessagePayload
{
    public ushort BytesLength { get; private set; } = bytesLen;
    public byte[] Ignored { get; private set; } = new byte[bytesLen];

    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBE(BytesLength));
        await stream.WriteAsync(Ignored);
    }

    public static async Task<PongPayload> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[2];
        await stream.ReadExactlyAsync(buffer);
        var bytesLength = EndianBitConverter.ToUInt16BE(buffer);

        var ignored = new byte[bytesLength];
        await stream.ReadExactlyAsync(ignored);

        return new PongPayload(bytesLength)
        {
            Ignored = ignored
        };
    }
}