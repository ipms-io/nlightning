namespace NLightning.Bolts.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a script validation error occurs.
/// </summary>
/// <remarks>
/// 
/// </remarks>
public class InvalidScriptException : ErrorException
{
    public InvalidScriptException(string message) : base(message) { }
    public InvalidScriptException(string message, Exception innerException) : base(message, innerException) { }
}