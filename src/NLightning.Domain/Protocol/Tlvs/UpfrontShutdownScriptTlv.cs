using System.Runtime.Serialization;
using NBitcoin;
using NLightning.Domain.Protocol.Constants;
using NLightning.Domain.Protocol.Models;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Tlvs;

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
    public Script ShutdownScriptPubkey { get; }

    public UpfrontShutdownScriptTlv(Script shutdownScriptPubkey) : base(TlvConstants.UpfrontShutdownScript)
    {
        ShutdownScriptPubkey = shutdownScriptPubkey;

        Value = shutdownScriptPubkey.ToBytes();
        Length = Value.Length;
    }

    /// <summary>
    /// Cast UpfrontShutdownScriptTlv from a BaseTlv.
    /// </summary>
    /// <param name="baseTlv">The baseTlv to cast from.</param>
    /// <returns>The cast UpfrontShutdownScriptTlv.</returns>
    /// <exception cref="SerializationException">Error casting UpfrontShutdownScriptTlv</exception>
    public static UpfrontShutdownScriptTlv FromTlv(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.UpfrontShutdownScript)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new UpfrontShutdownScriptTlv(new Script(baseTlv.Value));
    }
}