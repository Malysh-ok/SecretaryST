using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components.Exceptions;

namespace Presentation.DesignTime.Services;

/// <summary>
/// Заглушка провайдера сообщений для дизайн-тайма.
/// </summary>
/// <remarks>
/// Возвращает фиктивные сообщения об ошибках и исключения, не требующие реальных ресурсов локализации.
/// Используется только в среде разработки для создания экземпляров сервисов,
/// чтобы избежать исключений из-за отсутствия реального DI-контейнера.
/// </remarks>
public class DesignTimeErrorMsgProvider : IAppErrorMsgProvider
{
    /// <inheritdoc cref="IAppErrorMsgProvider.GetMessage" />
    public string GetMessage(string? code, params object[]? args)
    {
        return $"Design-time message: {code}";
    }

    /// <inheritdoc/>
    public AppException CreateException(string code, Exception? inner = null, 
        ExcptnTypeEnm excptnType = ExcptnTypeEnm.Error, params object[] args)
    {
        return AppException.CreateFromErrorCode($"Design-time: {code}", inner, null, excptnType);
    }

    /// <inheritdoc/>
    public AppException CreateFatalException(Exception? inner = null)
    {
        return AppException.CreateFromErrorCode($"Design-time: {AppErrorCodes.FatalError}", inner);
    }
}