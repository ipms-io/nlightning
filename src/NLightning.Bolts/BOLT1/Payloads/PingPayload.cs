namespace NLightning.Bolts.BOLT1.Payloads;

using Bolts.Interfaces;

public class PingPayload : IMessagePayload
{
    public const ushort MAX_LENGTH = 65531;

    public ushort NumPongBytes { get; private set; }
    public ushort BytesLength { get; private set; }
    public byte[] Ignored { get; private set; }

    public PingPayload()
    {
        // Get number of bytes at random between byte.MaxValue and ushort.MaxValue
        NumPongBytes = (ushort)new Random().Next(byte.MaxValue, MAX_LENGTH);
        BytesLength = (ushort)new Random().Next(byte.MaxValue, MAX_LENGTH);
        Ignored = new byte[BytesLength];
    }

    public async Task SerializeAsync(Stream stream)
    {
        await stream.WriteAsync(EndianBitConverter.GetBytesBE(NumPongBytes));
        await stream.WriteAsync(EndianBitConverter.GetBytesBE(BytesLength));
        await stream.WriteAsync(Ignored);
    }

    public static async Task<PingPayload> DeserializeAsync(Stream stream)
    {
        var buffer = new byte[2];
        await stream.ReadExactlyAsync(buffer);
        var numPongBytes = EndianBitConverter.ToUInt16BE(buffer);

        await stream.ReadExactlyAsync(buffer);
        var bytesLength = EndianBitConverter.ToUInt16BE(buffer);

        var ignored = new byte[bytesLength];
        await stream.ReadExactlyAsync(ignored);

        return new PingPayload
        {
            NumPongBytes = numPongBytes,
            BytesLength = bytesLength,
            Ignored = ignored
        };
    }
}