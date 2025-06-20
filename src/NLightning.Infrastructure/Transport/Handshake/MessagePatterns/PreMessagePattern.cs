namespace NLightning.Infrastructure.Transport.Handshake.MessagePatterns;

using Enums;

/// <summary>
/// A pre-message pattern is one of the following
/// sequences of tokens: "e", "s", "e, s", or empty.
/// </summary>
internal sealed class PreMessagePattern
{
    /// <summary>
    /// The "e" pre-message pattern.
    /// </summary>
    public static readonly PreMessagePattern E = new(Token.E);

    /// <summary>
    /// The "s" pre-message pattern.
    /// </summary>
    public static readonly PreMessagePattern S = new(Token.S);

    /// <summary>
    /// The "e, s" pre-message pattern.
    /// </summary>
    public static readonly PreMessagePattern Es = new(Token.E, Token.S);

    /// <summary>
    /// The empty pre-message pattern.
    /// </summary>
    public static readonly PreMessagePattern Empty = new();

    /// <summary>
    /// Gets the tokens of the pre-message pattern.
    /// </summary>
    internal IEnumerable<Token> Tokens { get; }

    private PreMessagePattern(params Token[] tokens)
    {
        Tokens = tokens;
    }
}