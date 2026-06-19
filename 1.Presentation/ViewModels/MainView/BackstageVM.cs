using System.Collections.ObjectModel;
using System.Windows.Input;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.UseCases.Services;
using Common.WpfModule.Components.Models;
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
    /// Данные для статус-бара.
    /// </summary>
    public StatusBarData StatusBarData { get; } = null!;

    
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
        StatusBarData statusBarData,
        ILogger logger,
        IExceptionsProvider exceptionsProvider,
        CompetitionDataService competitionDataService)
    {
        _logger = logger;
        StatusBarData = statusBarData;
        _logger = logger;
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
            _logger.Error(competitionsResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(OnGetAllCompetitionDataAsync));
        }
    }
    
    /// <summary>
    /// Добавление соревнования.
    /// </summary>
    private async Task OnAddCompetitionDataAsync()
    {
        var competitionResult = await _competitionDataService.AddCompetitionDataAsync(CompetitionDataCollection);

        if (competitionResult)
        {
            // Перезаписываем текущее соревнование
            CurrentCompetitionData = competitionResult.Value;
            
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(CompetitionDataCollection, CurrentCompetitionData));

            // TODO: Временно (без ожидания окончания)
            _ = StatusBarData.SetTextAsync("Добавили соревнование.", StatusBarData.StatusBarTextType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(competitionResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(competitionResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(OnAddCompetitionDataAsync));
        }
    }
    
    /// <summary>
    /// Удаление соревнования.
    /// </summary>
    private async Task OnRemoveCompetitionDataAsync()
    {
        var competitionResult = 
            await _competitionDataService.RemoveCompetitionDataAsync(CompetitionDataCollection, CurrentCompetitionData);
        
        if (competitionResult)
        {
            CurrentCompetitionData = competitionResult.Value;
            
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(CompetitionDataCollection, CurrentCompetitionData));
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarData.SetTextAsync("Удалили соревнование.", StatusBarData.StatusBarTextType.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(competitionResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(competitionResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(OnRemoveCompetitionDataAsync));
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