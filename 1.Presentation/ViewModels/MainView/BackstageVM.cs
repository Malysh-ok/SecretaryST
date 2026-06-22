using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.WpfModule.Components.Models;
using Common.BaseComponents.Components.Exceptions;
using Common.WpfModule.Components.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using ProblemDomain.Entities.CommonEntities;
using Serilog;

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для специфичного меню "File" (для Backstage).
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class BackstageVM : ObservableRecipient, IRecipient<CompetitionMessage>, IStatusBarDataProvider, IDisposable
{
    private readonly ILogger _logger = null!;
    private readonly IExceptionsProvider _exceptionsProvider = null!;
    private readonly AppSettingService _appSetting = null!;
    private readonly CompetitionDataService _competitionDataService = null!;
    
    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    public ObservableCollection<CompetitionData> CompetitionDataCollection { get; set; } = [];

    /// <summary>
    /// Текущее соревнование.
    /// </summary>
    public CompetitionData? CurrentCompetitionData { get; set; }
    
    /// <summary>
    /// Сервис статус-бара.
    /// </summary>
    public StatusBarService StatusBarService { get; } = null!;

    
    #region [---------- Команды ----------]
    
    /// <summary>
    /// Команда загрузки всех соревнований.
    /// </summary>
    public ICommand GetAllCompetitionsCommand { get; } = null!;
    
    /// <summary>
    /// Команда добавления соревнования.
    /// </summary>
    public ICommand AddCompetitionCommand { get; } = null!;

    /// <summary>
    /// Команда удаления соревнования.
    /// </summary>
    public ICommand RemoveCompetitionCommand { get; } = null!;

    #endregion

    
    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private BackstageVM()
    {
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public BackstageVM(
        StatusBarService statusBarService,
        ILogger logger,
        IExceptionsProvider exceptionsProvider,
        AppSettingService appSetting,
        CompetitionDataService competitionDataService)
    {
        StatusBarService = statusBarService;
        _logger = logger;
        _exceptionsProvider = exceptionsProvider;
        _appSetting = appSetting;
        _competitionDataService = competitionDataService;

        GetAllCompetitionsCommand = new AsyncRelayCommand(OnGetAllCompetitionDataAsync);
        AddCompetitionCommand = new AsyncRelayCommand(OnAddCompetitionDataAsync);
        RemoveCompetitionCommand =  new AsyncRelayCommand(OnRemoveCompetitionDataAsync);

        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
        // Инициализация
        _ = InitAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                var exception = new AppException(AppPhrases.UnknownError, t.Exception);
                _ = StatusBarData.SetTextAsync(exception.Message, 
                    StatusBarData.StatusBarTextType.Error, 0);
                _logger.Error(exception, "{class}.{method}",
                    typeof(BackstageVM), "CTOR");
            }
        });
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="CompetitionMessage"/>.
    /// </summary>
    public void Receive(CompetitionMessage message)
    {
        CurrentCompetitionData = message.CurrentCompetitionData;
    }

    /// <summary>
    /// Инициализация - получение всех необходимых данных.
    /// </summary>
    private async Task InitAsync()
    {
        await OnGetAllCompetitionDataAsync();
    }

    #region [---------- Функции для команд ----------]

    /// <summary>
    /// Получение данных о всех соревнованиях.
    /// </summary>
    private async Task OnGetAllCompetitionDataAsync()
    {
        // Получаем список соревнований
        var competitionsResult = 
            await _competitionDataService.GetAllCompetitionsDataAsync(CompetitionDataCollection);
        if (competitionsResult)
        {
            if (CompetitionDataCollection.Any())
            {
                // Если коллекция не пуста
                
                var newCurrentCompetitionData = CompetitionDataCollection.First();
                if (CurrentCompetitionData != null)
                {
                    var id = CurrentCompetitionData.Id;
                    // Если CurrentCompetitionData создано (не из репозитория) -
                    // присваиваем последний, иначе - находим по id
                    newCurrentCompetitionData = id == 0 
                        ? CompetitionDataCollection.Last()  
                        : CompetitionDataCollection.First(c => c.Id == id);
                }
                CurrentCompetitionData = newCurrentCompetitionData;
            }
            else
            {
                CurrentCompetitionData = null;
            }

            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(CompetitionDataCollection, CurrentCompetitionData));
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(competitionsResult.Excptn?.Message,
                StatusBarData.StatusBarTextType.Error, 0);
            _ = StatusBarService.SetTextAsync(competitionsResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(competitionsResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(OnGetAllCompetitionDataAsync));
        }
    }
    
    /// <summary>
    /// Добавление соревнования.
    /// </summary>
    private async Task OnAddCompetitionDataAsync()
    {
        Exception? exception = null;
        try
        {
            // Получаем
            var competitionResult = await _competitionDataService.AddCompetitionDataAsync(CompetitionDataCollection);
            if (! competitionResult)
            {
                // Неудачное получение данных из репозитория
                exception = competitionResult.Excptn;
                return;
            }
            
            // Сохраняем изменения
            var intResult = await _competitionDataService.SaveCompetitionDataAsync();
            if (! intResult)
            {
                // Неудачное сохранение в репозитории
                exception = competitionResult.Excptn;
                return;
            }

            CurrentCompetitionData = competitionResult.Value;
            
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(CompetitionDataCollection, CurrentCompetitionData));
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarService.SetTextAsync("Добавили соревнование.", BaseException.ExcptnType.Error);
        }
        finally
        {
            if (exception != null)
            {
                // Пишем в статус-бар и лог об ошибке
                _ = StatusBarService.SetTextAsync(exception.Message,
                    BaseException.ExcptnType.Error, 0);
                _logger.Error(exception,
                    "{class}.{method}.",
                    typeof(SettingVM), nameof(OnAddCompetitionDataAsync));
            }
        }
    }
    
    /// <summary>
    /// Удаление соревнования.
    /// </summary>
    private async Task OnRemoveCompetitionDataAsync()
    {
        Exception? exception = null;
        try
        {
            // TODO: Временно, возможно будет отдельное окно
            var result = MessageBox.Show("Вы уверены, что хотите удалить соревнование?", _appSetting.AppName, 
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.No)
                return;
            
            // Удаляем
            var competitionResult = 
                await _competitionDataService.RemoveCompetitionDataAsync(CompetitionDataCollection, CurrentCompetitionData);
            if (! competitionResult)
            {
                // Неудачное удаление данных в репозитории
                exception = competitionResult.Excptn;
                return;
            }
            
            // Сохраняем изменения
            var intResult = await _competitionDataService.SaveCompetitionDataAsync();
            if (! intResult)
            {
                // Неудачное сохранение в репозитории
                exception = competitionResult.Excptn;
                return;
            }

            CurrentCompetitionData = competitionResult.Value;
            
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(CompetitionDataCollection, CurrentCompetitionData));
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarService.SetTextAsync("Удалили соревнование.", BaseException.ExcptnType.Error);
        }
        finally
        {
            if (exception != null)
            {
                // Пишем в статус-бар и лог об ошибке
                _ = StatusBarService.SetTextAsync(exception.Message,
                    BaseException.ExcptnType.Error, 0);
                _logger.Error(exception,
                    "{class}.{method}.",
                    typeof(SettingVM), nameof(OnRemoveCompetitionDataAsync));
            }
        }
    }

    #endregion
    
    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose"/>
    private void Dispose(bool disposing)
    {
    }
    
    /// <inheritdoc />
    ~BackstageVM() => Dispose(false);
}