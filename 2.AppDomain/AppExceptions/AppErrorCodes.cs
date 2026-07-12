namespace AppDomain.AppExceptions;

/// <summary>
/// Содержит строковые коды ошибок предметной области приложения (2.AppDomain).
/// </summary>
/// <remarks>
/// Коды используются для идентификации типа ошибки и последующей локализации сообщения.
/// Коды должны быть уникальными в пределах домена.
/// </remarks>
public static class AppErrorCodes
{
    public const string AppDirsCreateError = nameof(AppDirsCreateError);
    public const string FatalError = nameof(FatalError);
    public const string LanguageNotFound = nameof(LanguageNotFound);
    public const string LocalizingError = nameof(LocalizingError);
    public const string UnknownError = nameof(UnknownError);
}