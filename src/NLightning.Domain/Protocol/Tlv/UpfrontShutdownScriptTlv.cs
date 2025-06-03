using NLightning.Domain.Bitcoin.ValueObjects;

namespace NLightning.Domain.Protocol.Tlv;

using Constants;

/// <summary>
/// Upfront Shutdown Script TLV.
/// </summary>
/// <remarks>
/// The upfront shutdown script TLV is used in the AcceptChannel2Message to communicate the script to be used when the
/// channel is being closed.
/// </remarks>
public class UpfrontShutdownScriptTlv : BaseTlv
{
    /// <summary>
    /// The shutdown script to be used when closing the channel
    /// </summary>
    public BitcoinScript ShutdownScriptPubkey { get; }

    public UpfrontShutdownScriptTlv(BitcoinScript shutdownScriptPubkey) : base(TlvConstants.UpfrontShutdownScript)
    {
        ShutdownScriptPubkey = shutdownScriptPubkey;

        Value = shutdownScriptPubkey.Value;
        Length = Value.Length;
    }
}