using System.Diagnostics;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.WpfModule.Ui.Services;
using Presentation.ViewModels._Contracts;
using Serilog;

namespace Presentation.ViewModels.Common;

/// <summary>
/// Общие методы для различных ViewModel.
/// </summary>
public class ViewModelHelper
{
    /// <summary>
    /// Обрабатывает исключение (если есть) и асинхронно запускает задачу,
    /// перехватывая её ошибки без блокировки вызывающего потока.
    /// </summary>
    /// <param name="exceptionsProvider">Провайдер с исключением и флагом фатальности.</param>
    /// <param name="taskFactory">Фабрика, создающая асинхронную задачу.</param>
    /// <param name="statusBarService">Сервис статус-бара.</param>
    /// <param name="logger">Логгер.</param>
    /// <remarks>
    /// Метод определяет имя вызывающего класса и метода через стек. При наличии исключения оно
    /// отображается в статус-баре и логируется. Если исключение отсутствует или не фатально,
    /// запускается задача, ошибки которой обрабатываются через ContinueWith.
    /// </remarks>
    public static void HandleExceptionsProvider(
            IExceptionsProvider exceptionsProvider, 
            Func<Task> taskFactory,
            StatusBarService statusBarService,
            ILogger logger)
    {
        // Получаем имя вызывающего метода и его имя класса
        var frame = new StackFrame(1);  // вызывающий метод
        var method = frame.GetMethod();
        var className = method?.DeclaringType?.FullName ?? "Unknown";
        var methodName = (method?.Name ?? "Unknown").RemoveChars(".");
        
        // 1. Обработка синхронного исключения (если есть)
        if (exceptionsProvider.Exception is not null)
        {
            _ = statusBarService.SetTextAsync(exceptionsProvider.Exception.Message, 
                BaseException.ExcptnType.Error, 0);
            logger.Error(exceptionsProvider.Exception, "{class}.{method}",
                className, methodName);
        }
        
        // 2. Если исключения нет или оно не фатально — запускаем асинхронную задачу
        if (exceptionsProvider.Exception is null || exceptionsProvider is {Exception: not null, IsFatal: false})
        {
            // Запускаем задачу без ожидания, обрабатываем ошибки через ContinueWith
            _ = taskFactory().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = new AppException(AppPhrases.UnknownError, t.Exception);
                    _ = statusBarService.SetTextAsync(exception.Message, 
                        BaseException.ExcptnType.Error, 0);
                    logger.Error(exception, "{class}.{method}",
                        className, methodName);
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}