using System.Diagnostics;

namespace NLightning.Infrastructure.Transport.Handshake.MessagePatterns;

using Domain.Crypto.Constants;
using Enums;

/// <summary>
/// A message pattern is some sequence of tokens from
/// the set ("e", "s", "ee", "es", "se", "ss", "psk").
/// </summary>
internal sealed class MessagePattern
{
    internal MessagePattern(params Token[] tokens)
    {
        Debug.Assert(tokens != null);
        Debug.Assert(tokens.Length > 0);

        Tokens = tokens;
    }

    /// <summary>
    /// Gets the tokens of the message pattern.
    /// </summary>
    internal IEnumerable<Token> Tokens { get; }

    /// <summary>
    /// Calculate the message overhead in bytes (i.e. the
    /// total size of all transmitted keys and AEAD tags).
    /// </summary>
    internal int Overhead(int dhLen, bool hasKey)
    {
        // Overhead always includes the Version length, which is 1 byte
        var overhead = 1;

        foreach (var token in Tokens)
        {
            switch (token)
            {
                case Token.E:
                    overhead += dhLen;
                    break;
                case Token.S:
                    overhead += hasKey ? dhLen + CryptoConstants.Chacha20Poly1305TagLen : dhLen;
                    break;
                case Token.Ee:
                case Token.Se:
                case Token.Es:
                case Token.Ss:
                default:
                    hasKey = true;
                    break;
            }
        }

        return hasKey ? overhead + CryptoConstants.Chacha20Poly1305TagLen : overhead;
    }
}