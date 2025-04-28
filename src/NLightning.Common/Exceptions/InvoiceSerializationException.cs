using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NLightning.Common.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an error occurs during invoice serialization or deserialization.
/// </summary>
[ExcludeFromCodeCoverage]
public class InvoiceSerializationException : ErrorException
{
    public InvoiceSerializationException(string message) : base(message)
    { }
    public InvoiceSerializationException(string message, Exception innerException) : base(message, innerException)
    { }

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(argument))
        {
            ThrowNullOrEmptyException(argument, paramName);
        }
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(argument))
        {
            ThrowNullOrWhiteSpaceException(argument, paramName);
        }
    }

    private static void ThrowNullOrEmptyException(string? argument, string? paramName)
    {
        try
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
        }
        catch (Exception e)
        {
            throw new InvoiceSerializationException("Error serializing invoice", e);
        }
    }

    private static void ThrowNullOrWhiteSpaceException(string? argument, string? paramName)
    {
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
        }
        catch (Exception e)
        {
            throw new InvoiceSerializationException("Error serializing invoice", e);
        }
    }
}