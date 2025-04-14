using Serilog;
using ILogger = NLightning.Common.Interfaces.ILogger;

namespace NLightning.NLTG;

public class SerilogLogger : ILogger
{
    public void Verbose(string messageTemplate)
    {
        Log.Logger.Verbose(messageTemplate);
    }

    public void Verbose<T>(string messageTemplate, T propertyValue)
    {
        Log.Logger.Verbose(messageTemplate, propertyValue);
    }

    public void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Log.Logger.Verbose(messageTemplate, propertyValue0, propertyValue1);
    }

    public void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Log.Logger.Verbose(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    public void Verbose(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Verbose(messageTemplate, propertyValues);
    }

    public void Verbose(Exception? exception, string messageTemplate)
    {
        Log.Logger.Verbose(exception, messageTemplate);
    }

    public void Verbose<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Log.Logger.Verbose(exception, messageTemplate, propertyValue);
    }

    public void Verbose<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Log.Logger.Verbose(exception, messageTemplate, propertyValue0, propertyValue1);
    }

    public void Verbose<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
        T2 propertyValue2)
    {
        Log.Logger.Verbose(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    public void Verbose(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Verbose(exception, messageTemplate, propertyValues);
    }

    public void Debug(string messageTemplate)
    {
        Log.Logger.Debug(messageTemplate);
    }

    public void Debug<T>(string messageTemplate, T propertyValue)
    {
        Log.Logger.Debug(messageTemplate, propertyValue);
    }

    public void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Log.Logger.Debug(messageTemplate, propertyValue0, propertyValue1);
    }

    public void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Log.Logger.Debug(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    public void Debug(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Debug(messageTemplate, propertyValues);
    }

    public void Debug(Exception? exception, string messageTemplate)
    {
        Log.Logger.Debug(exception, messageTemplate);
    }

    public void Debug<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Log.Logger.Debug(exception, messageTemplate, propertyValue);
    }

    public void Debug<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Log.Logger.Debug(exception, messageTemplate, propertyValue0, propertyValue1);
    }

    public void Debug<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
        T2 propertyValue2)
    {
        Log.Logger.Debug(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    public void Debug(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Debug(exception, messageTemplate, propertyValues);
    }

    public void Information(string messageTemplate)
    {
        Log.Logger.Information(messageTemplate);
    }

    public void Information<T>(string messageTemplate, T propertyValue)
    {
        Log.Logger.Information(messageTemplate, propertyValue);
    }

    public void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Log.Logger.Information(messageTemplate, propertyValue0, propertyValue1);
    }

    public void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        Log.Logger.Information(messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    public void Information(string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Information(messageTemplate, propertyValues);
    }

    public void Information(Exception? exception, string messageTemplate)
    {
        Log.Logger.Information(exception, messageTemplate);
    }

    public void Information<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        Log.Logger.Information(exception, messageTemplate, propertyValue);
    }

    public void Information<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        Log.Logger.Information(exception, messageTemplate, propertyValue0, propertyValue1);
    }

    public void Information<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
        T2 propertyValue2)
    {
        Log.Logger.Information(exception, messageTemplate, propertyValue0, propertyValue1, propertyValue2);
    }

    public void Information(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Information(exception, messageTemplate, propertyValues);
    }

    public void Information(Exception? exception, object? sender, string? messageTemplate)
    {
        Log.Logger.Information(exception, $"[{sender?.GetType().Name}] {messageTemplate}");
    }

    public void Information<T>(Exception? exception, object? sender, string messageTemplate, T propertyValue)
    {
        Log.Logger.Information(exception, $"[{sender?.GetType().Name}] {messageTemplate}", propertyValue);
    }

    public void Information<T0, T1>(Exception? exception, object? sender, string messageTemplate, T0 propertyValue0,
        T1 propertyValue1)
    {
        Log.Logger.Information(exception, $"[{sender?.GetType().Name}] {messageTemplate}", propertyValue0,
            propertyValue1);
    }

    public void Information<T0, T1, T2>(Exception? exception, object? sender, string messageTemplate, T0 propertyValue0,
        T1 propertyValue1, T2 propertyValue2)
    {
        Log.Logger.Information(exception, $"[{sender?.GetType().Name}] {messageTemplate}", propertyValue0,
            propertyValue1, propertyValue2);
    }

    public void Information(Exception? exception, object? sender, string messageTemplate, params object?[]? propertyValues)
    {
        Log.Logger.Information(exception, $"[{sender?.GetType().Name}] {messageTemplate}", propertyValues);
    }

    public void Warning(string messageTemplate)
    {
        Log.Logger.Warning(messageTemplate);
    }

    public void Warning<T>(string messageTemplate, T propertyValue)
    {
        Log.Logger.Warning(messageTemplate, propertyValue);
    }

    public void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        throw new NotImplementedException();
    }

    public void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        throw new NotImplementedException();
    }

    public void Warning(string messageTemplate, params object?[]? propertyValues)
    {
        throw new NotImplementedException();
    }

    public void Warning(Exception? exception, string messageTemplate)
    {
        throw new NotImplementedException();
    }

    public void Warning<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        throw new NotImplementedException();
    }

    public void Warning<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        throw new NotImplementedException();
    }

    public void Warning<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
        T2 propertyValue2)
    {
        throw new NotImplementedException();
    }

    public void Warning(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        throw new NotImplementedException();
    }

    public void Error(string messageTemplate)
    {
        throw new NotImplementedException();
    }

    public void Error<T>(string messageTemplate, T propertyValue)
    {
        throw new NotImplementedException();
    }

    public void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        throw new NotImplementedException();
    }

    public void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        throw new NotImplementedException();
    }

    public void Error(string messageTemplate, params object?[]? propertyValues)
    {
        throw new NotImplementedException();
    }

    public void Error(Exception? exception, string messageTemplate)
    {
        throw new NotImplementedException();
    }

    public void Error<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        throw new NotImplementedException();
    }

    public void Error<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        throw new NotImplementedException();
    }

    public void Error<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
        T2 propertyValue2)
    {
        throw new NotImplementedException();
    }

    public void Error(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        throw new NotImplementedException();
    }

    public void Fatal(string messageTemplate)
    {
        throw new NotImplementedException();
    }

    public void Fatal<T>(string messageTemplate, T propertyValue)
    {
        throw new NotImplementedException();
    }

    public void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        throw new NotImplementedException();
    }

    public void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2)
    {
        throw new NotImplementedException();
    }

    public void Fatal(string messageTemplate, params object?[]? propertyValues)
    {
        throw new NotImplementedException();
    }

    public void Fatal(Exception? exception, string messageTemplate)
    {
        throw new NotImplementedException();
    }

    public void Fatal<T>(Exception? exception, string messageTemplate, T propertyValue)
    {
        throw new NotImplementedException();
    }

    public void Fatal<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1)
    {
        throw new NotImplementedException();
    }

    public void Fatal<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1,
        T2 propertyValue2)
    {
        throw new NotImplementedException();
    }

    public void Fatal(Exception? exception, string messageTemplate, params object?[]? propertyValues)
    {
        throw new NotImplementedException();
    }
}