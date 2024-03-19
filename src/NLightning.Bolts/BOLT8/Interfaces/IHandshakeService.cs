namespace NLightning.Bolts.BOLT8.Interfaces;

/// <summary>
/// The Handshake Service
/// </summary>
internal interface IHandshakeService : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the handshake is in the initiator role.
    /// </summary>
    bool IsInitiator { get; }

    /// <summary>
    /// The Transport that is going to be returned by the handshake after all steps has been completed
    /// </summary>
    ITransport? Transport { get; }

    /// <summary>
    /// Perform the next step in the handshake
    /// </summary>
    /// <param name="inMessage">Byte[] representation of In Message</param>
    /// <param name="outMessage">The buffer to write the message to</param>
    /// <returns>Number of bytes written to outMessage</returns>
    int PerformStep(ReadOnlySpan<byte> inMessage, Span<byte> outMessage);
}