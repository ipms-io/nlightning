namespace NLightning.Domain.Protocol.Payloads;

using Messages;
using Interfaces;

/// <summary>
/// Represents a Pong payload.
/// </summary>
/// <remarks>
/// A Pong payload is used to respond to a Ping payload.
/// </remarks>
/// <seealso cref="PongMessage"/>
public class PongPayload : IMessagePayload
{
    /// <summary>
    /// The length of the ignored bytes.
    /// </summary>
    public ushort BytesLength { get; }

    /// <summary>
    /// The ignored bytes.
    /// </summary>
    public byte[] Ignored { get; }

    public PongPayload(ushort bytesLen)
    {
        BytesLength = bytesLen;
        Ignored = new byte[bytesLen];
        
        // Fill the ignored bytes with random data
        new Random().NextBytes(Ignored);
    }
}