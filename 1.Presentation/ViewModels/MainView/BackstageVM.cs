using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.WpfModule.Ui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Messages;
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

        GetAllCompetitionsCommand = new AsyncRelayCommand(GetAllCompetitionsAsync);
        CreateCompetitionCommand = new AsyncRelayCommand(CreateCompetitionAsync);
        RemoveCompetitionCommand =  new AsyncRelayCommand(RemoveCompetitionAsync);

        ShowAppSettingCommand = new RelayCommand(ShowAppSetting);
        
        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
        // Обработка исключений "сверху", запуск инициализации если исключений нет
        ViewModelHelper.HandleExceptionsProvider(
            exceptionsProvider, InitAsync, StatusBarService, _logger);
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="CompetitionMessage"/>.
    /// </summary>
    public void Receive(CompetitionMessage message)
    {
        CurrentCompetition = message.CurrentCompetition;
    }

    /// <summary>
    /// Инициализация - получение всех необходимых данных.
    /// </summary>
    private async Task InitAsync()
    {
        await GetAllCompetitionsAsync();
    }

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

    
    #region [---------- Соревнования ----------]
    
    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    public ObservableCollection<CompetitionData> Competitions { get; set; } = [];

    /// <summary>
    /// Текущее соревнование.
    /// </summary>
    public CompetitionData? CurrentCompetition { get; set; }
    
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
    /// Команда создания соревнования.
    /// </summary>
    public ICommand CreateCompetitionCommand { get; } = null!;

    /// <summary>
    /// Команда удаления соревнования.
    /// </summary>
    public ICommand RemoveCompetitionCommand { get; } = null!;

    #endregion

        /// <summary>
    /// Получение (обновление) коллекции соревнований.
    /// </summary>
    private async Task GetAllCompetitionsAsync()
    {
        // Получаем список соревнований
        var competitionsResult = 
            await _competitionDataService.GetAllCompetitionsDataAsync(Competitions);
        if (competitionsResult)
        {
            if (Competitions.Any())
            {
                // Если коллекция не пуста
                
                var newCurrentCompetitionData = Competitions.First();
                if (CurrentCompetition != null)
                {
                    var id = CurrentCompetition.Id;
                    // Если CurrentCompetitionData создано (не из репозитория) -
                    // присваиваем последний, иначе - находим по id
                    newCurrentCompetitionData = id == 0 
                        ? Competitions.Last()  
                        : Competitions.First(c => c.Id == id);
                }
                CurrentCompetition = newCurrentCompetitionData;
            }
            else
            {
                CurrentCompetition = null;
            }

            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(Competitions, CurrentCompetition));
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(competitionsResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(competitionsResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetAllCompetitionsAsync));
        }
    }
    
    /// <summary>
    /// Создание соревнования.
    /// </summary>
    private async Task CreateCompetitionAsync()
    {
        Exception? exception = null;
        try
        {
            // Получаем
            var competitionResult = await _competitionDataService.CreateCompetitionDataAsync(Competitions);
            if (! competitionResult)
            {
                // Неудачное получение данных из репозитория
                exception = competitionResult.Excptn;
                return;
            }

            CurrentCompetition = competitionResult.Value;
            
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(Competitions, CurrentCompetition));
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarService.SetTextAsync("Добавили соревнование.", BaseException.ExcptnType.Info);
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
                    typeof(SettingVM), nameof(CreateCompetitionAsync));
            }
        }
    }
    
    /// <summary>
    /// Удаление соревнования.
    /// </summary>
    private async Task RemoveCompetitionAsync()
    {
        Exception? exception = null;
        try
        {
            // TODO: Временно, возможно будет отдельное окно
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить соревнование '{CurrentCompetition!.ShortName}'?",
                _appSetting.AppName, 
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.No)
                return;
            
            // Удаляем
            var competitionResult = 
                await _competitionDataService.RemoveCompetitionDataAsync(Competitions, CurrentCompetition);
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
                exception = intResult.Excptn;
                return;
            }

            CurrentCompetition = competitionResult.Value;
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarService.SetTextAsync("Удалили соревнование.", BaseException.ExcptnType.Warning);
        }
        finally
        {
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(Competitions, CurrentCompetition));
            
            if (exception != null)
            {
                // Пишем в статус-бар и лог об ошибке
                _ = StatusBarService.SetTextAsync(exception.Message,
                    BaseException.ExcptnType.Error, 0);
                _logger.Error(exception,
                    "{class}.{method}.",
                    typeof(SettingVM), nameof(RemoveCompetitionAsync));
            }
        }
    }
    
    #endregion


    #region [---------- Работа с приложением ----------]

    /// <summary>
    /// Команда показа окна настроек приложения.
    /// </summary>
    public ICommand ShowAppSettingCommand { get; } = null!;

    /// <summary>
    /// Показываем окно настроек приложения (используя сообщение для code-behind представления MainView).
    /// </summary>
    private void ShowAppSetting()
    {
        // Отправляем сообщение
        Messenger.Send(new OpenAppSettingMessage());
    }
    
    #endregion
}