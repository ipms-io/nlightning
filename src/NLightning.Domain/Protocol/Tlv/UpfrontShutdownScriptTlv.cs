using NBitcoin;

namespace NLightning.Domain.Protocol.Tlv;

using Constants;
using Models;

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
    public Script ShutdownScriptPubkey { get; internal set; }

    public UpfrontShutdownScriptTlv(Script shutdownScriptPubkey) : base(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
    {
        ShutdownScriptPubkey = shutdownScriptPubkey;

        Value = shutdownScriptPubkey.ToBytes();
        Length = Value.Length;
    }
}