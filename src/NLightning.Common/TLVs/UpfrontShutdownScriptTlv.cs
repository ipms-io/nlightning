using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Upfront Shutdown Script TLV.
/// </summary>
/// <remarks>
/// The upfront shutdown script TLV is used in the AcceptChannel2Message to communicate the script to be used when the
/// channel is being closed.
/// </remarks>
public class UpfrontShutdownScriptTlv : Tlv
{
    /// <summary>
    /// The shutdown script to be used when closing the channel
    /// </summary>
    public Script ShutdownScriptPubkey { get; }

    public UpfrontShutdownScriptTlv(Script shutdownScriptPubkey) : base(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
    {
        ShutdownScriptPubkey = shutdownScriptPubkey;

        Value = shutdownScriptPubkey.ToBytes();
        Length = Value.Length;
    }

    /// <summary>
    /// Cast UpfrontShutdownScriptTlv from a Tlv.
    /// </summary>
    /// <param name="tlv">The tlv to cast from.</param>
    /// <returns>The cast UpfrontShutdownScriptTlv.</returns>
    /// <exception cref="SerializationException">Error casting UpfrontShutdownScriptTlv</exception>
    public static UpfrontShutdownScriptTlv FromTlv(Tlv tlv)
    {
        if (tlv.Type != TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (tlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new UpfrontShutdownScriptTlv(new Script(tlv.Value));
    }
}