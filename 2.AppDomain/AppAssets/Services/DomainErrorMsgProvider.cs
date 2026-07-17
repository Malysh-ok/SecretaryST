using System.Globalization;
using System.Resources;
using AppDomain.AppAssets.Strings;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using ProblemDomain.ProblemExceptions;
using ProblemDomain.UseCases._Contracts;

namespace AppDomain.AppAssets.Services;

/// <summary>
/// Обеспечивает получение сообщений по кодам ошибок и их применение к исключениям
/// для предметной области (3.ProblemDomain) и для предметной области приложения (2.AppDomain).
/// </summary>
/// <remarks>
/// Реализация читает ресурсы <see cref="AppPhrases"/>.
/// </remarks>
public class DomainErrorMsgProvider : IAppErrorMsgProvider, IProblemErrorMsgProvider
{
    private readonly ResourceManager _resourceManager;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public DomainErrorMsgProvider()
    {
        AppPhrases.Culture = CultureInfo.CurrentUICulture;       // устанавливаем языковой стандарт для фраз
        _resourceManager = AppPhrases.ResourceManager;
    }

    /// <inheritdoc cref="IAppErrorMsgProvider" />
    public string? GetMessage(string? errCode, params object[]? args)
    {
        if (errCode.IsNullOrEmpty())
            return null;
        
        var format = _resourceManager.GetString(errCode!);
        
        return format != null && args is { Length: > 0 } 
            ? string.Format(format, args) 
            : format;
    }
    
    /// <inheritdoc/>
    ProblemException IProblemErrorMsgProvider.CreateException(string code, Exception? inner, ExcptnTypeEnm excptnType, params object[] args)
    {
        var message = GetMessage(code, args);
        return ProblemException.CreateFromErrorCode(code, inner, message, excptnType);
    }

    /// <inheritdoc/>
    AppException IAppErrorMsgProvider.CreateException(string code, Exception? inner, ExcptnTypeEnm excptnType, params object[] args)
    {
        var message = GetMessage(code, args);
        return AppException.CreateFromErrorCode(code, inner, message, excptnType);
    }

    /// <inheritdoc/>
    AppException IAppErrorMsgProvider.CreateFatalException(Exception? inner)
    {
        const string code = AppErrorCodes.FatalError;
        var message = GetMessage(code);
        return AppException.CreateFromErrorCode(code, inner, message);
    }
}