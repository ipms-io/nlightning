namespace NLightning.Bolts.BOLT1.Payloads;

public class FakePingPayload : PingPayload
{
    public FakePingPayload(int numPongBytes, int bytesLength)
    {
        NumPongBytes = (ushort)numPongBytes;
        BytesLength = (ushort)bytesLength;
        Ignored = new byte[BytesLength];
    }
}