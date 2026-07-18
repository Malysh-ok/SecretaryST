using System.Globalization;
using System.Resources;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using DataAccess.DataAccessAssets.Strings;
using DataAccess.DataAccessExceptions;

namespace DataAccess.DataAccessAssets.Services;

/// <summary>
/// Обеспечивает получение сообщений по кодам ошибок и их применение к исключениям
/// для слоя доступа к данным (4.DataAccess).
/// </summary>
/// <remarks>
/// Реализация читает ресурсы <see cref="DbPhrases"/>.
/// </remarks>
public class DataAccessErrorMsgProvider
{
    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public DataAccessErrorMsgProvider()
    {
        _resourceManager = DbPhrases.ResourceManager;
    }
    
    /// <summary>
    /// Возвращает сообщение по коду ошибки с подстановкой аргументов.
    /// </summary>
    /// <param name="errCode">Код ошибки.</param>
    /// <param name="args">Аргументы для форматирования сообщения.</param>
    // ReSharper disable once MemberCanBePrivate.Global
    public string? GetMessage(string? errCode, params object[]? args)
    {
        if (errCode.IsNullOrEmpty())
            return null;
        
        var culture = DbPhrases.Culture ?? CultureInfo.CurrentUICulture;
        var format = _resourceManager.GetString(errCode!, culture);
        
        return format != null && args is { Length: > 0 } 
            ? string.Format(format, args) 
            : format;
    }

    /// <summary>
    /// Создаёт <see cref="DataAccessException"/> с сообщением по коду ошибки.
    /// </summary>
    /// <param name="code">Код ошибки.</param>
    /// <param name="inner">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="excptnType">Тип исключения.</param>
    /// <param name="args">Аргументы для форматирования сообщения.</param>
    public DataAccessException CreateException(string code, Exception? inner = null,
        ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error, params object[] args)
    {
        var message = GetMessage(code, args);
        return DataAccessException.CreateFromErrorCode(code, inner, message, excptnType);
    }
}