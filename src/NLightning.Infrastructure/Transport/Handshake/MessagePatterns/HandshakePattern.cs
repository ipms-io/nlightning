using System.Diagnostics;

namespace NLightning.Infrastructure.Transport.Handshake.MessagePatterns;

using Enums;

/// <summary>
/// A <see href="https://noiseprotocol.org/noise.html#handshake-patterns">handshake pattern</see>
/// consists of a pre-message pattern for the initiator, a pre-message pattern for the responder,
/// and a sequence of message patterns for the actual handshake messages.
/// </summary>
internal sealed class HandshakePattern
{
    /// <summary>
    /// Gets the pre-message pattern for the initiator.
    /// </summary>
    public PreMessagePattern Initiator { get; }

    /// <summary>
    /// Gets the pre-message pattern for the responder.
    /// </summary>
    public PreMessagePattern Responder { get; }

    /// <summary>
    /// Gets the sequence of message patterns for the handshake messages.
    /// </summary>
    public IEnumerable<MessagePattern> Patterns { get; }

    /// <summary>
    /// XK():
    /// <para>- ← s</para>
    /// <para>- ...</para>
    /// <para>- → e, es</para>
    /// <para>- ← e, ee</para>
    /// <para>- → s, se</para>
    /// </summary>
    public static readonly HandshakePattern Xk = new(
        PreMessagePattern.Empty,
        PreMessagePattern.S,
        new MessagePattern(Token.E, Token.Es),
        new MessagePattern(Token.E, Token.Ee),
        new MessagePattern(Token.S, Token.Se)
    );

    internal HandshakePattern(PreMessagePattern initiator, PreMessagePattern responder, params MessagePattern[] patterns)
    {
        Debug.Assert(initiator != null);
        Debug.Assert(responder != null);
        Debug.Assert(patterns != null);
        Debug.Assert(patterns.Length > 0);

        Initiator = initiator;
        Responder = responder;
        Patterns = patterns;
    }
}