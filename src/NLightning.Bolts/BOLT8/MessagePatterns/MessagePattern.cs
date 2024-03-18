using System.Diagnostics;

namespace NLightning.Bolts.BOLT8.MessagePatterns;

using Ciphers;
using Enums;

/// <summary>
/// A message pattern is some sequence of tokens from
/// the set ("e", "s", "ee", "es", "se", "ss", "psk").
/// </summary>
public sealed class MessagePattern
{
	internal MessagePattern(params Token[] tokens)
	{
		Debug.Assert(tokens != null);
		Debug.Assert(tokens.Length > 0);

		Tokens = tokens;
	}

	internal MessagePattern(IEnumerable<Token> tokens)
	{
		Debug.Assert(tokens != null);
		Debug.Assert(tokens.Any());

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
		// OIverhead always includes the Version lenght, which is 1 byte
		int overhead = 1;

		foreach (var token in Tokens)
		{
			if (token == Token.E)
			{
				overhead += dhLen;
			}
			else if (token == Token.S)
			{
				overhead += hasKey ? dhLen + ChaCha20Poly1305.TAG_SIZE : dhLen;
			}
			else
			{
				hasKey = true;
			}
		}

		return hasKey ? overhead + ChaCha20Poly1305.TAG_SIZE : overhead;
	}

	private static IEnumerable<T> Prepend<T>(IEnumerable<T> source, T element)
	{
		yield return element;

		foreach (var item in source)
		{
			yield return item;
		}
	}

	private static IEnumerable<T> Append<T>(IEnumerable<T> source, T element)
	{
		foreach (var item in source)
		{
			yield return item;
		}

		yield return element;
	}
}