using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;

namespace Presentation.DesignTime.Services;

/// <summary>
/// Заглушка провайдера сообщений для дизайн-тайма.
/// </summary>
public class DesignTimeErrorMsgProvider : IAppErrorMsgProvider
{
    /// <inheritdoc cref="IAppErrorMsgProvider" />
    public string GetMessage(string? code, params object[]? args)
    {
        return $"[DesignTime] {code}";
    }

    /// <inheritdoc/>
    public AppException CreateException(string code, Exception? inner = null, params object[] args)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public AppException CreateFatalException(Exception? inner = null)
    {
        throw new NotImplementedException();
    }
}