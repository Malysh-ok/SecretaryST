using System.Windows;
using System.Windows.Input;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
using Common.WpfModule.Ui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Shared.Infrastructure;
using Presentation.ViewModels.Shared.Infrastructure._Contracts;
using Presentation.ViewModels.Shared.Messages;
using Presentation.ViewModels.Shared.Models;
using Presentation.ViewModels.Shared.Models._Contracts;
using ProblemDomain.UseCases.Services;
using Serilog;

namespace Presentation.ViewModels.Main;

/// <summary>
/// ViewModel для специфичного меню "File" (для Backstage).
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class BackstageVM : ObservableRecipient, IRecipient<CompetitionMessage>, IDisposable
{
    // ReSharper disable once NotAccessedField.Local
    private readonly ICompetitionChangeNotifier _competitionChangeNotifier;
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;
    private readonly AppSettingsService _appSettingsService;
    private readonly StatusBarService _statusBarService;
    private readonly CompetitionService _competitionService;
    private readonly ViewModelHelper _viewModelHelper;

    /// <summary>
    /// Конструктор.
    /// </summary>
    public BackstageVM(
        ICompetitionChangeNotifier competitionChangeNotifier,
        ILogger logger,
        IExceptionsProvider exceptionsProvider,
        IAppErrorMsgProvider appErrorMsgProvider,
        AppSettingsService appSettingsService,
        StatusBarService statusBarService,
        CompetitionService competitionService)
    {
        _competitionChangeNotifier = competitionChangeNotifier;
        _appErrorMsgProvider = appErrorMsgProvider;
        _appSettingsService = appSettingsService;
        _statusBarService = statusBarService;
        _competitionService = competitionService;
        _viewModelHelper = new ViewModelHelper(logger, appErrorMsgProvider, statusBarService);

        UpdateCompetitionsCommand = new AsyncRelayCommand(UpdateCompetitionsAsync);
        CreateCompetitionCommand = new AsyncRelayCommand(CreateCompetitionAsync);
        RemoveCompetitionCommand =  new AsyncRelayCommand(RemoveCompetitionAsync);

        ShowAppSettingCommand = new RelayCommand(ShowAppSetting);
        
        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
        // Обработка исключений "сверху", запуск инициализации если исключений нет
        _viewModelHelper.HandleExceptionsProvider(exceptionsProvider, InitAsync);
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
        await UpdateCompetitionsAsync();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose"/>
    // ReSharper disable once UnusedParameter.Local
    private void Dispose(bool disposing)
    {
    }
    
    /// <inheritdoc />
    ~BackstageVM() => Dispose(false);

    
    #region [---------- Соревнования ----------]
    
    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    private ObservableCollectionEx<CompetitionObservable> Competitions { get; set; } = [];

    /// <summary>
    /// Текущее соревнование.
    /// </summary>
    private CompetitionObservable? CurrentCompetition { get; set; }

    #region [---------- Команды ----------]
    
    /// <summary>
    /// Команда получения (обновления) коллекции соревнований.
    /// </summary>
    public ICommand UpdateCompetitionsCommand { get; }
    
    /// <summary>
    /// Команда создания соревнования.
    /// </summary>
    public ICommand CreateCompetitionCommand { get; }

    /// <summary>
    /// Команда удаления соревнования.
    /// </summary>
    public ICommand RemoveCompetitionCommand { get; }

    #endregion

    /// <summary>
    /// Получение (обновление) коллекции соревнований.
    /// </summary>
    private async Task UpdateCompetitionsAsync()
    {
        // Получаем список соревнований
        var competitionsResult = 
            await _competitionService.GetAllCompetitionsAsync();
        if (competitionsResult)
        {
            // Перезаписываем коллекцию соревнований
            var newList = new List<CompetitionObservable>();
            competitionsResult.Value.ForEach(item =>
            {
                var co = CompetitionObservable.CreatePartial(item);
                newList.Add(co);
            });
            Competitions.ClearAndAddRange(newList);
            
            // Получаем текущее соревнование, если коллекция не пуста
            if (Competitions.Any())
            {
                var id = CurrentCompetition?.Competition.Id;
                // Находим соревнование по сохраненному id, если не нашли - выбираем первое
                CurrentCompetition = Competitions.FirstOrDefault(co => co.Competition.Id == id) ?? Competitions.First();
            }
            else
            {
                CurrentCompetition =  null;
            }

            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(Competitions, CurrentCompetition));
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(competitionsResult.Excptn, 
                this.ToString(), nameof(UpdateCompetitionsAsync));
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
            // Создаем соревнование в репозитории с сохранением изменений
            var competitionResult = await _competitionService.CreateCompetitionAsync();
            if (! competitionResult)
            {
                // Неудачное получение данных из репозитория
                exception = competitionResult.Excptn;
                return;
            }

            // Создаем Observable-соревнование и сохраняем его в коллекции
            CurrentCompetition = CompetitionObservable.CreatePartial(competitionResult.Value!);
            Competitions.Add(CurrentCompetition);
            
            // Посылаем сообщение о создании соревнования
            Messenger.Send(new AllCompetitionsMessage(Competitions, CurrentCompetition));
            
            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Добавили соревнование.", ExcptnTypeEnm.Info);
        }
        finally
        {
            // Пишем в статус-бар и лог об ошибке при ее наличии
            _viewModelHelper.HandleException(exception, 
                this.ToString(), nameof(CreateCompetitionAsync));
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
                $"Вы уверены, что хотите удалить соревнование '{CurrentCompetition!.Competition.ShortName}'?",
                _appSettingsService.AppName, 
                MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result == MessageBoxResult.No)
                return;
            
            // Удаляем из репозитория с сохранением изменений
            var competitionResult = await _competitionService.RemoveCompetitionAsync(CurrentCompetition.Competition);
            if (! competitionResult)
            {
                // Неудачное удаление данных в репозитории
                exception = competitionResult.Excptn;
                return;
            }
            
            // Удаляем из коллекции
            var index = Competitions.IndexOf(CurrentCompetition);
            Competitions.Remove(CurrentCompetition);
            if (index == Competitions.Count)
                index = Competitions.Count - 1;
            else if (index == 0)
                index = -1;
            CurrentCompetition = Competitions.GetOrDefault(index);

            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Удалили соревнование.", ExcptnTypeEnm.Warning);
        }
        finally
        {
            // Посылаем сообщение о загрузке соревнований
            Messenger.Send(new AllCompetitionsMessage(Competitions, CurrentCompetition));
            
            // Пишем в статус-бар и лог об ошибке при ее наличии
            _viewModelHelper.HandleException(exception, 
                this.ToString(), nameof(RemoveCompetitionAsync));
        }
    }
    
    #endregion


    #region [---------- Работа с приложением ----------]

    /// <summary>
    /// Команда показа окна настроек приложения.
    /// </summary>
    public ICommand ShowAppSettingCommand { get; }

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