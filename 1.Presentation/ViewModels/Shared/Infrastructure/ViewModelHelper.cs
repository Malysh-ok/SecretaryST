using System.Diagnostics;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Services;
using Presentation.ViewModels.Shared.Infrastructure._Contracts;
using Serilog;

namespace Presentation.ViewModels.Shared.Infrastructure;

/// <summary>
/// Общие методы для различных ViewModel.
/// </summary>
/// <param name="logger">Логгер.</param>
/// <param name="statusBarService">Сервис статус-бара.</param>
/// <param name="appErrorMsgProvider">Провайдер сообщений об ошибках доменного слоя.</param>
// ReSharper disable once ClassNeverInstantiated.Global
public class ViewModelHelper(
    ILogger logger,
    IAppErrorMsgProvider appErrorMsgProvider,
    StatusBarService? statusBarService)
{
    /// <summary>
    /// Обрабатывает исключение (если есть) и асинхронно запускает задачу,
    /// перехватывая её ошибки без блокировки вызывающего потока.
    /// </summary>
    /// <param name="exceptionsProvider">Провайдер с исключением и флагом фатальности.</param>
    /// <param name="taskFactory">Фабрика, создающая асинхронную задачу.</param>
    /// <remarks>
    /// Метод определяет имя вызывающего класса и метода через стек. При наличии исключения оно
    /// отображается в статус-баре и логируется. Если исключение отсутствует или не фатально,
    /// запускается задача, ошибки которой обрабатываются через ContinueWith.
    /// </remarks>
    public void HandleExceptionsProvider(IExceptionsProvider exceptionsProvider,
        Func<Task> taskFactory)
    {
        // Получаем имя вызывающего метода и его имя класса
        var frame = new StackFrame(1);  // вызывающий метод
        var method = frame.GetMethod();
        var className = method?.DeclaringType?.FullName ?? "Unknown";
        var methodName = (method?.Name ?? "Unknown").RemoveChars(".");
        
        // 1. Обработка синхронного исключения (если есть)
        if (exceptionsProvider.Exception is not null)
        {
            // Пишем в статус-бар и лог об ошибке
            HandleException(exceptionsProvider.Exception, className, methodName);
        }
        
        // 2. Если исключения нет или оно не фатально — запускаем асинхронную задачу
        if (exceptionsProvider.Exception is null || exceptionsProvider is {Exception: not null, IsFatal: false})
        {
            // Запускаем задачу без ожидания, обрабатываем ошибки через ContinueWith
            _ = taskFactory().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    // Пишем в статус-бар и лог об ошибке
                    var exception = appErrorMsgProvider.CreateException(AppErrorCodes.UnknownError, t.Exception);
                    HandleException(exception, className, methodName);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }

    /// <summary>
    /// Обрабатывает исключение (если есть).
    /// </summary>
    /// <param name="exception">Исключение.</param>
    /// <param name="callerClassName">Имя вызывающего класса.</param>
    /// <param name="callerMethodName">Имя вызывающего метода.</param>
    /// <remarks>
    /// При наличии исключения оно отображается в статус-баре и логируется.
    /// </remarks>
    public void HandleException(Exception? exception,
        string? callerClassName,
        string? callerMethodName)
    {
        if (exception == null) 
            return;
        
        // Пишем в статус-бар и лог об ошибке
        logger.Error(exception, 
            "{CallerClassName}.{CallerMethodName}", callerClassName, callerMethodName);
        if (statusBarService != null)
            _ = statusBarService.SetTextAsync(exception.Message,
                BaseException.ExcptnType.Error, 0);
    }
}