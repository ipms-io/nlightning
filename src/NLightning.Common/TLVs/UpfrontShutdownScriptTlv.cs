using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Common.TLVs;

using Constants;
using Types;

/// <summary>
/// Funding Output Contribution TLV.
/// </summary>
/// <remarks>
/// The funding output contribution TLV is used in the TxInitRbfMessage to communicate the funding output contribution in satoshis.
/// </remarks>
public class UpfrontShutdownScriptTlv : Tlv
{
    /// <summary>
    /// The amount being contributed in satoshis
    /// </summary>
    public Script? ShutdownScriptPubkey { get; private set; }

    public UpfrontShutdownScriptTlv() : base(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
    { }
    public UpfrontShutdownScriptTlv(Script shutdownScriptPubkey) : base(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
    {
        ShutdownScriptPubkey = shutdownScriptPubkey;
    }

    /// <summary>
    /// Serialize the TLV to a stream
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="SerializationException">Error serializing TLV or any of it's parts</exception>
    public new async Task SerializeAsync(Stream stream)
    {
        var scriptBytes = ShutdownScriptPubkey?.ToBytes() ?? throw new NullReferenceException("ShutdownScriptPubkey was null");

        Length = scriptBytes.Length;
        Value = scriptBytes;

        await base.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserialize a UpfrontShutdownScriptTlv from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpfrontShutdownScriptTlv.</returns>
    /// <exception cref="SerializationException">Error deserializing UpfrontShutdownScriptTlv</exception>
    public static new async Task<UpfrontShutdownScriptTlv> DeserializeAsync(Stream stream)
    {
        var tlv = await Tlv.DeserializeAsync(stream) as UpfrontShutdownScriptTlv ?? throw new SerializationException("Invalid TLV type");

        if (tlv.Type != TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
        {
            throw new SerializationException("Invalid TLV type");
        }

        if (tlv.Length == 0)
        {
            throw new SerializationException("Invalid length");
        }

        tlv.ShutdownScriptPubkey = new Script(tlv.Value);

        return tlv;
    }
}