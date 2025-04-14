namespace NLightning.Common.Interfaces;

using Attributes;

public interface ILogger
{
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose(string messageTemplate);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose<T>(string messageTemplate, T propertyValue);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose(string messageTemplate, params object?[]? propertyValues);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose(Exception? exception, string messageTemplate);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose<T>(Exception? exception, string messageTemplate, T propertyValue);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Verbose(Exception? exception, string messageTemplate, params object?[]? propertyValues);

    void Debug(string messageTemplate);
    void Debug<T>(string messageTemplate, T propertyValue);
    void Debug<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Debug<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Debug(string messageTemplate, params object?[]? propertyValues);
    void Debug(Exception? exception, string messageTemplate);
    void Debug<T>(Exception? exception, string messageTemplate, T propertyValue);
    void Debug<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Debug<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Debug(Exception? exception, string messageTemplate, params object?[]? propertyValues);

    [MessageTemplateFormatMethod("messageTemplate")]
    void Information(string messageTemplate);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T>(string messageTemplate, T propertyValue);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information(string messageTemplate, params object?[]? propertyValues);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information(Exception? exception, string messageTemplate);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T>(Exception? exception, string messageTemplate, T propertyValue);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information(Exception? exception, string messageTemplate, params object?[]? propertyValues);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information(Exception? exception, object? sender, string? messageTemplate);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T>(Exception? exception, object? sender, string messageTemplate, T propertyValue);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T0, T1>(Exception? exception, object? sender, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information<T0, T1, T2>(Exception? exception, object? sender, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    [MessageTemplateFormatMethod("messageTemplate")]
    void Information(Exception? exception, object? sender, string messageTemplate, params object?[]? propertyValues);

    void Warning(string messageTemplate);
    void Warning<T>(string messageTemplate, T propertyValue);
    void Warning<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Warning<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Warning(string messageTemplate, params object?[]? propertyValues);
    void Warning(Exception? exception, string messageTemplate);
    void Warning<T>(Exception? exception, string messageTemplate, T propertyValue);
    void Warning<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Warning<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Warning(Exception? exception, string messageTemplate, params object?[]? propertyValues);

    void Error(string messageTemplate);
    void Error<T>(string messageTemplate, T propertyValue);
    void Error<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Error<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Error(string messageTemplate, params object?[]? propertyValues);
    void Error(Exception? exception, string messageTemplate);
    void Error<T>(Exception? exception, string messageTemplate, T propertyValue);
    void Error<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Error<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Error(Exception? exception, string messageTemplate, params object?[]? propertyValues);

    void Fatal(string messageTemplate);
    void Fatal<T>(string messageTemplate, T propertyValue);
    void Fatal<T0, T1>(string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Fatal<T0, T1, T2>(string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Fatal(string messageTemplate, params object?[]? propertyValues);
    void Fatal(Exception? exception, string messageTemplate);
    void Fatal<T>(Exception? exception, string messageTemplate, T propertyValue);
    void Fatal<T0, T1>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
    void Fatal<T0, T1, T2>(Exception? exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
    void Fatal(Exception? exception, string messageTemplate, params object?[]? propertyValues);
}