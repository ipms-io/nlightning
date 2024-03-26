namespace NLightning.Bolts.Exceptions;

public class InvalidMessageException(string message) : Exception
{
    public new string Message = message;
    public object? ExpectedType;
    public object? ReceivedType;

    public InvalidMessageException(string message, object received, object? expected = null) : this(message)
    {
        ReceivedType = received;
        ExpectedType = expected;

        // Check if message has placeholder for types and format the string
        if (message.Contains("{0}"))
        {
            if (message.Contains("{1}"))
            {
                Message = string.Format(message, received, expected);
            }
            else
            {
                Message = string.Format(message, received);
            }
        }
        else
        {
            Message = message;
        }
    }
}