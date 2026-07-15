using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AppDomain.AppEntities;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Cogs.Collections;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
using Common.WpfModule.Components.Wrappers;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views._Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Shared.Infrastructure;
using Presentation.ViewModels.Shared.Infrastructure._Contracts;
using Presentation.ViewModels.Shared.Messages;
using Presentation.ViewModels.Shared.Models;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using ProblemDomain.UseCases.Services;
using Serilog;

namespace Presentation.ViewModels.Main;

/// <summary>
/// ViewModel для меню ленты "Настройки".
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class SettingVM : ObservableRecipient, 
    IRecipient<LocalizationMessage>, IRecipient<AllCompetitionsMessage>, IDisposable
{
    private readonly IViewWithResources _view = null!;
    private readonly IAppErrorMsgProvider _appErrorMsgProvider = null!;
    private readonly AppSettingService _appSettingService = null!;
    private readonly StatusBarService _statusBarService = null!;
    private readonly CompetitionDataService _competitionDataService = null!;
    private readonly RefereeService _refereeService = null!;
    private readonly SportEventService _sportEventService = null!;
    private readonly LocalizationHelper _localizationHelper = null!;
    private readonly ViewModelHelper _viewModelHelper = null!;

    /// <summary>
    /// Текущая локализация.
    /// </summary>
    public Lang? CurrLang
    {
        get;
        private set => SetProperty(ref field, value);
    }


    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private SettingVM()
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public SettingVM(
        IViewWithResources view,
        ILogger logger,
        IExceptionsProvider exceptionsProvider,
        IAppErrorMsgProvider appErrorMsgProvider,
        AppSettingService appSettingService,
        StatusBarService statusBarService,
        CompetitionDataService competitionDataService,
        RefereeService refereeService,
        SportEventService sportEventService)
    {
        _view = view;
        _appErrorMsgProvider = appErrorMsgProvider;
        _appSettingService = appSettingService;
        _statusBarService = statusBarService;
        _competitionDataService = competitionDataService;
        _refereeService = refereeService;
        _sportEventService =  sportEventService;
        _localizationHelper = new LocalizationHelper(appSettingService);
        _viewModelHelper = new ViewModelHelper(logger, appErrorMsgProvider, statusBarService);

        // Соревнования
        CreateConductingOrganizationCommand = new RelayCommand(CreateConductingOrganization);
        RemoveConductingOrganizationCommand = new RelayCommand(RemoveConductingOrganization);
        GetCompetitionCommand = new AsyncRelayCommand<int>(
            GetCompetitionDataAsync,
            id => id > 0 // CanExecute: только если Id корректный);
        );
        SaveCompetitionCommand = new AsyncRelayCommand(SaveCompetitionAsync);
        
        // Виды программы
        GetSportEventsCommand = new AsyncRelayCommand(GetSportEventObservablesAsync);
        CreateSportEventCommand = new AsyncRelayCommand(CreateSportEventAsync);
        RemoveSportEventCommand =  new RelayCommand(RemoveSportEvent);
        
        // Судьи
        GetRefereesCommand = new AsyncRelayCommand(GetRefereesAsync);
        CreateRefereeCommand = new AsyncRelayCommand(CreateRefereeAsync);
        RemoveRefereeCommand = new RelayCommand(RemoveReferee);
        RenumberRefereesCommand = new RelayCommand(RenumberReferee);
        
        // Подписываемся на получение сообщений
        Messenger.Register<LocalizationMessage>(this);
        Messenger.Register<AllCompetitionsMessage>(this);

        // Получение языка локализации из настроек
        var localization = appSettingService.AppLocalization;
        var langName = localization.GetLangFromSetting();
        CurrLang = localization.SetCurrentLangFromName(langName);
        
        // Оповещаем все представления (окна) приложения о смене языка локализации
        Messenger.Send(new LocalizationMessage(CurrLang, null));
        
        // Обработка исключений "сверху", запуск инициализации если исключений нет
        _viewModelHelper.HandleExceptionsProvider(exceptionsProvider, InitAsync);
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    public async void Receive(LocalizationMessage message)
    {
        try
        {
            // Устанавливаем текущий язык
            CurrLang = message.Lang;
            
            // Локализация представления асинхронно в UI-потоке, но без блокировки
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (! _localizationHelper.LocalizeView(_view, message.Lang))
                {
                    // Если локализовать не получилось - возвращаем предыдущий язык
                    CurrLang = message.OldLang;
                }

            }, DispatcherPriority.Normal);
        
            // Дополнительные обновления данных
            await GetDisciplineGroupsAsync();
            await GetDisciplineSubGroupsAsync();
        }
        catch (Exception ex)
        {
            // Пишем в статус-бар и лог об ошибке
            var exception =_appErrorMsgProvider.CreateException(AppErrorCodes.LocalizingError, ex, this.GetType().Name);

            _viewModelHelper.HandleException(exception, this.ToString(), nameof(Receive));
        }
    }

    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="AllCompetitionsMessage"/>.
    /// </summary>
    public void Receive(AllCompetitionsMessage message)
    {
        Competitions = message.Competitions;
        CurrentCompetition = message.CurrentCompetition;
    }
        
    /// <summary>
    /// Инициализация - получение всех необходимых данных.
    /// </summary>
    private async Task InitAsync()
    {
        // TODO: Возможно нужно сделать проверку результатов вызовов, и если false - как-то реагировать
        await GetDetailedCompetitionStatusesAsync();
        await GetRefereeLevels();
        await GetRefereeJobTitles();
        await GetDisciplineGroupsAsync();
        await GetDisciplineSubGroupsAsync();
        await GetDisciplinesAsync();
        await GetDifficultiesDicAsync();
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
        if (disposing)
        {
            _appSettingService.SaveConfig();
        }
    }
    
    /// <inheritdoc />
    ~SettingVM() => Dispose(false);


    #region [---------- Данные о соревнованиях ----------]
        
    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    public ObservableCollection<CompetitionData> Competitions { get; private set; } = [];

    private CompetitionData? _currentCompetition;
    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionData? CurrentCompetition
    {
        get => _currentCompetition;
        set
        {
            if (SetProperty(ref _currentCompetition, value) && value != null)
            {
                // При смене выбранного соревнования подгружаем навигационные свойства
                _ = GetCompetitionDataAsync(value.Id);
                
                // Посылаем сообщение об изменении текущего соревнования
                Messenger.Send(new CompetitionMessage(CurrentCompetition));
            }
        }
    }

    /// <summary>
    /// Коллекция статусов и обобщенных наименований соревнований.
    /// </summary>
    public ObservableCollectionEx<DetailedCompetitionStatus> DetailedCompetitionStatuses { get; set; } = [];

    /// <summary>
    /// Коллекция проводящих организаций.
    /// </summary>
    public ObservableCollectionEx<StringItem> ConductingOrganizations { get; set; } = [];

    #region [---------- Команды ----------]
        
    /// <summary>
    /// Команда сохранения соревнования (включая зависимые сущности).
    /// </summary>
    public ICommand SaveCompetitionCommand { get; } = null!;

    /// <summary>
    /// Команда получения соревнования.
    /// </summary>
    public IAsyncRelayCommand<int> GetCompetitionCommand { get; } = null!;
    
    /// <summary>
    /// Команда создания проводящей организации.
    /// </summary>
    public ICommand CreateConductingOrganizationCommand { get; } = null!;

    /// <summary>
    /// Команда удаления проводящей организации.
    /// </summary>
    public ICommand RemoveConductingOrganizationCommand { get; } = null!;

    #endregion

    /// <summary>
    /// Сохранение соревнования (включая зависимые сущности).
    /// </summary>
    private async Task SaveCompetitionAsync()
    {
        Exception? exception = null;
        try
        {
            // Сохраняем проводящие организации
            var intResult = _competitionDataService.SetConductingOrganizations(ConductingOrganizations, CurrentCompetition);
            if (! intResult)
            {
                exception = intResult.Excptn;
                return;
            }
        
            // Сохраняем изменения
            intResult = await _competitionDataService.SaveCompetitionDataAsync();
            if (! intResult)
            {
                exception = intResult.Excptn;
                return;
            }

            // TODO: возможно изменим - Обновляем соревнование, хотя бы потому, чтобы обновилась коллекция сорев, при изменении ShortName одного из них
            if (CurrentCompetition != null)
                // await GetCompetitionDataAsync(CurrentCompetition.Id);
                CurrentCompetition = CurrentCompetition;
        }
        finally
        {
            // Пишем в статус-бар и лог об ошибке при ее наличии
            _viewModelHelper.HandleException(exception, this.ToString(), nameof(SaveCompetitionAsync));
        }
    }

    /// <summary>
    /// Получение (обновление) статусов и наименований соревнований.
    /// </summary>
    private async Task GetDetailedCompetitionStatusesAsync()
    {
        var detailedCompetitionsStatusesResult = await _competitionDataService.GetDetailedCompetitionsStatusesAsync();

        if (detailedCompetitionsStatusesResult)
        {
            // Перезаписываем коллекцию статусов и наименований соревнований
            DetailedCompetitionStatuses.Clear();
            DetailedCompetitionStatuses.AddRange(detailedCompetitionsStatusesResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(detailedCompetitionsStatusesResult.Excptn, 
                this.ToString(), nameof(GetDetailedCompetitionStatusesAsync));
        }
    }

    /// <summary>
    /// Создание проводящей организации.
    /// </summary>
    private void CreateConductingOrganization()
    {
        // Создаем новую организацию
        var newIndexResult = _competitionDataService.CreateConductingOrganization(ConductingOrganizations, 
            ConductingOrganizations.SelectedIndex);
        if (newIndexResult)
        {
            // Перезаписываем индекс
            ConductingOrganizations.SelectedIndex = newIndexResult.Value;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(newIndexResult.Excptn, 
                this.ToString(), nameof(CreateConductingOrganization));
        }
    }
    
    /// <summary>
    /// Удаление проводящей организации.
    /// </summary>
    private void RemoveConductingOrganization()
    {
        // Удаляем организацию
        var newIndexResult = _competitionDataService.RemoveConductingOrganization(ConductingOrganizations, 
            ConductingOrganizations.SelectedIndex);

        if (newIndexResult)
        {
            // Перезаписываем индекс
            ConductingOrganizations.SelectedIndex = newIndexResult.Value;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(newIndexResult.Excptn, 
                this.ToString(), nameof(RemoveConductingOrganization));
        }
    }
    
    /// <summary>
    /// Получение полных данных о соревновании.
    /// </summary>
    private async Task GetCompetitionDataAsync(int id)
    {
        Exception? exception = null;
        try
        {
            // Получаем
            var competitionDataResult = await _competitionDataService.GetCompetitionDataAsync(id, true);
            if (! competitionDataResult)
            {
                // Неудачное получение данных из репозитория
                exception = competitionDataResult.Excptn;
                return;
            }

            // Заменяем объект в коллекции
            var existing = Competitions.FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                var index = Competitions.IndexOf(existing);
                Competitions[index] = competitionDataResult.Value!;    // замена
            }
            else
            {
                Competitions.Add(competitionDataResult.Value!);        // добавление
            }
            
            // Заменяем текущие соревнования (меняем через поле, чтобы не вызвать данный метод повторно)
            SetProperty(ref _currentCompetition, competitionDataResult.Value, nameof(CurrentCompetition));
            
            // Посылаем сообщение об изменении текущего соревнования
            Messenger.Send(new CompetitionMessage(CurrentCompetition));
            
            // Заполняем проводящие организации
            var intResult = _competitionDataService.GetConductingOrganizations(
                ConductingOrganizations, CurrentCompetition);
            if (! intResult)
            {
                exception = intResult.Excptn;
            }
            
            // Обновляем список судей
            _ = GetRefereesAsync();
            
            // Обновляем список видов программы
            _ = GetSportEventObservablesAsync();
        }
        finally
        {
            // Пишем в статус-бар и лог об ошибке при ее наличии
            _viewModelHelper.HandleException(exception, this.ToString(), nameof(GetCompetitionDataAsync));
        }
    }
    
    #endregion

    #region [---------- Виды программы ----------]

    /// <summary>
    /// Словарь текстовых значений для трудностей вида программы.
    /// </summary>
    private readonly ObservableDictionary<(DisciplineGroupEnm, Difficulty.IdEnm), string> _difficultiesDic = [];

    /// <summary>
    /// Коллекция Observable-видов программы.
    /// </summary>
    public ObservableCollectionEx<SportEventObservable> SportEventObservables { get; set; } = [];
    
    /// <summary>
    /// Коллекция групп дисциплин (включая null).
    /// </summary>
    public ObservableCollectionEx<KeyValuePair<DisciplineGroup?, string>> DisciplineGroupsWithNull { get; } = [];
    
    /// <summary>
    /// Коллекция подгрупп дисциплин (включая null).
    /// </summary>
    public ObservableCollectionEx<KeyValuePair<DisciplineSubGroup?, string>> DisciplineSubGroupsWithNull { get; } = [];

    /// <summary>
    /// Группа спортивных дисциплин, ограничивающая выбор возможных дисциплин.
    /// </summary>
    private DisciplineGroup? FilteringDisciplineGroup =>
        DisciplineGroupsWithNull.SelectedIndex >= 0
            ? DisciplineGroupsWithNull[DisciplineGroupsWithNull.SelectedIndex].Key
            : null;
    
    /// <summary>
    /// Подгруппа спортивных дисциплин, ограничивающая выбор возможных дисциплин.
    /// </summary>
    private DisciplineSubGroup? FilteringDisciplineSubGroup =>
        DisciplineSubGroupsWithNull.SelectedIndex >= 0
            ? DisciplineSubGroupsWithNull[DisciplineSubGroupsWithNull.SelectedIndex].Key
            : null;

    /// <summary>
    /// Коллекция дисциплин.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public ObservableCollectionEx<Discipline> Disciplines { get; } = [];
    
    /// <summary>
    /// Коллекция доступных для выбора дисциплин. 
    /// </summary>
    public ObservableCollection<Discipline> AvailableDisciplines
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];
    
    /// <summary>
    /// Признак фильтрации дисциплин по группам дисциплин.
    /// </summary>
    public bool IsFilteredByDisciplineGroup
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsFilteredByDisciplineSubGroup));
            }
        }
    } = true;

    /// <summary>
    /// Признак фильтрации дисциплин по подгруппам дисциплин.
    /// </summary>
    public bool IsFilteredByDisciplineSubGroup => ! IsFilteredByDisciplineGroup;

    
    #region [---------- Команды ----------]
    
    /// <summary>
    /// Команда загрузки списка видов программы.
    /// </summary>
    public ICommand GetSportEventsCommand { get; } = null!;
    
    /// <summary>
    /// Команда создания вида программы.
    /// </summary>
    public ICommand CreateSportEventCommand { get; } = null!;

    /// <summary>
    /// Команда удаления вида программы.
    /// </summary>
    public ICommand RemoveSportEventCommand { get; } = null!;

    #endregion
    
    /// <summary>
    /// Получение (обновление) коллекции <see cref="SportEventObservable"/>.
    /// </summary>
    private async Task GetSportEventObservablesAsync()
    {
        var difficultiesResult = await _sportEventService.GetSportEventsAsync(CurrentCompetition!);
        if (difficultiesResult)
        {
            // Перезаписываем коллекцию
            SportEventObservables.Clear();
            difficultiesResult.Value.ForEach(item => SportEventObservables.Add(
                new SportEventObservable(_sportEventService, _difficultiesDic, item)));
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(difficultiesResult.Excptn, 
                this.ToString(), nameof(GetSportEventObservablesAsync));
        }
    }

    /// <summary>
    /// Создание Observable-вида программы.
    /// </summary>
    private async Task CreateSportEventAsync()
    {
        var sportEventResult = await _sportEventService.CreateSportEventAsync(CurrentCompetition);
        if (sportEventResult)
        {
            // Индекс
            var index = SportEventObservables.SelectedIndex;
            if (index < 0)
                index = SportEventObservables.Count;

            SportEventObservables.Insert(index, 
                new SportEventObservable(_sportEventService, _difficultiesDic, sportEventResult.Value!));
                
            // Увеличиваем индекс
            SportEventObservables.SelectedIndex = ++index;
            
            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Добавили вид программы.", BaseException.ExcptnType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(sportEventResult.Excptn, this.ToString(), nameof(CreateSportEventAsync));
        }
    }

    /// <summary>
    /// Удаление Observable-вида программы.
    /// </summary>
    private void RemoveSportEvent()
    {
        // Индекс
        var index = SportEventObservables.SelectedIndex;
        if (index < 0)
            return;
        
        // Удаляем из репозитория
        var intResult = _sportEventService.RemoveSportEvent(SportEventObservables[index].SportEvent);
        if (intResult)
        {

            // Удаляем из коллекции
            SportEventObservables.RemoveAt(index);

            // Обновляем индекс
            SportEventObservables.SelectedIndex = index == 0 ? 0 : --index;

            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Удалили вид программы.", BaseException.ExcptnType.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(intResult.Excptn, this.ToString(), nameof(RemoveSportEvent));
        }
    }

    /// <summary>
    /// Обновление коллекции доступных дисциплин (callback).
    /// </summary>
    private void UpdateAvailableDisciplines(int _)
    {
        // Фильтруем дисциплины по ограничивающим подгруппе и группе дисциплин
        AvailableDisciplines = new ObservableCollection<Discipline>(_sportEventService.GetAvailableDisciplines(
            Disciplines, FilteringDisciplineSubGroup, FilteringDisciplineGroup));
    }

    /// <summary>
    /// Получение (обновление) коллекции групп дисциплин.
    /// </summary>
    private async Task GetDisciplineGroupsAsync()
    {
        var disciplineGroupsResult = await _sportEventService.GetDisciplineGroupsAsync();
        if (disciplineGroupsResult)
        {
            // Сохраняем индекс
            var index = DisciplineGroupsWithNull.SelectedIndex;
            if (index < 0 || index > DisciplineGroupsWithNull.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            DisciplineGroupsWithNull.Clear();
            DisciplineGroupsWithNull.Add(new KeyValuePair<DisciplineGroup?, string>(null, 
                _localizationHelper.GetLocalizedString(_view, "NullDisplayText")));
            disciplineGroupsResult.Value.ForEach(item => DisciplineGroupsWithNull.Add(
                new KeyValuePair<DisciplineGroup?, string>(item, item.ToString())));

            // Устанавливаем индекс
            DisciplineGroupsWithNull.SelectedIndex = index;
            
            // Устанавливаем функцию обратного вызова при изменении индекса коллекции
            DisciplineGroupsWithNull.OnSelectedIndexChanged(UpdateAvailableDisciplines);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(disciplineGroupsResult.Excptn, 
                this.ToString(), nameof(GetDisciplineGroupsAsync));        
        }
    }
    
    /// <summary>
    /// Получение (обновление) коллекции подгрупп дисциплин.
    /// </summary>
    private async Task GetDisciplineSubGroupsAsync()
    {
        var disciplineSubGroupsResult = await _sportEventService.GetDisciplineSubGroupsAsync();
        if (disciplineSubGroupsResult)
        {
            // Сохраняем индекс
            var index = DisciplineSubGroupsWithNull.SelectedIndex;
            if (index < 0 || index > DisciplineSubGroupsWithNull.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            DisciplineSubGroupsWithNull.Clear();
            DisciplineSubGroupsWithNull.Add(new KeyValuePair<DisciplineSubGroup?, string>(null, 
                _localizationHelper.GetLocalizedString(_view, "NullDisplayText")));
            disciplineSubGroupsResult.Value.ForEach(item => 
                DisciplineSubGroupsWithNull.Add(new KeyValuePair<DisciplineSubGroup?, string>(item, item.ToString())));

            // Устанавливаем индекс
            DisciplineSubGroupsWithNull.SelectedIndex = index;
            
            // Устанавливаем функцию обратного вызова при изменении индекса коллекции
            DisciplineSubGroupsWithNull.OnSelectedIndexChanged(UpdateAvailableDisciplines);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(disciplineSubGroupsResult.Excptn, 
                this.ToString(), nameof(GetDisciplineSubGroupsAsync));        
        }
    }
    
    /// <summary>
    /// Получение (обновление) коллекции дисциплин.
    /// </summary>
    private async Task GetDisciplinesAsync()
    {
        var disciplinesResult = await _sportEventService.GetDisciplinesAsync();
        if (disciplinesResult)
        {
            // Перезаписываем коллекцию дисциплин
            Disciplines.Clear();
            Disciplines.AddRange(disciplinesResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(disciplinesResult.Excptn, this.ToString(), nameof(GetDisciplinesAsync));        
        }
    }
    
    /// <summary>
    /// Получение (обновление) словаря текстовых значений
    /// для трудностей видов программы: (DisciplineGroup, Difficulty) -> string
    /// </summary>
    private async Task GetDifficultiesDicAsync()
    {
        var difficultiesResult = await _sportEventService.GetAllDifficultiesAsync();
        if (difficultiesResult)
        {
            // Перезаписываем коллекцию
            _difficultiesDic.Clear();
            _difficultiesDic.AddRange(difficultiesResult.Value!);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(difficultiesResult.Excptn, 
                this.ToString(), nameof(GetDifficultiesDicAsync));        
        }
    }
    
    #endregion
    
    #region [---------- Судьи ----------]

    /// <summary>
    /// Коллекция судейских категорий.
    /// </summary>
    public ObservableCollectionEx<RefereeLevel> RefereeLevels { get; } = [];
    
    /// <summary>
    /// Коллекция должностей.
    /// </summary>
    public ObservableCollectionEx<RefereeJobTitle> RefereeJobTitles { get; } = [];

    /// <summary>
    /// Коллекция судей.
    /// </summary>
    public ObservableCollectionEx<Referee> Referees { get; set; } = [];

    #region [---------- Команды ----------]

    /// <summary>
    /// Команда загрузки списка судей.
    /// </summary>
    public ICommand GetRefereesCommand { get; } = null!;
    
    /// <summary>
    /// Команда создания судьи.
    /// </summary>
    public ICommand CreateRefereeCommand { get; } = null!;

    /// <summary>
    /// Команда удаления судьи.
    /// </summary>
    public ICommand RemoveRefereeCommand { get; } = null!;
    
    /// <summary>
    /// Команда перенумерования судей.
    /// </summary>
    public ICommand RenumberRefereesCommand { get; } = null!;

    #endregion

    /// <summary>
    /// Получение судейских категорий.
    /// </summary>
    private async Task GetRefereeLevels()
    {
        var refereeLevelsResult = await _refereeService.GetRefereeLevelsAsync();
        if (refereeLevelsResult)
        {
            // Перезаписываем коллекцию судейских категорий
            RefereeLevels.Clear();
            RefereeLevels.AddRange(refereeLevelsResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeLevelsResult.Excptn, 
                this.ToString(), nameof(GetRefereeLevels));        
        }
    }
    
    /// <summary>
    /// Получение судейских должностей.
    /// </summary>
    private async Task GetRefereeJobTitles()
    {
        var refereeJobTitlesResult = await _refereeService.GetRefereeJobTitlesAsync();
        if (refereeJobTitlesResult)
        {
            // Перезаписываем коллекцию судейских должностей
            RefereeJobTitles.Clear();
            RefereeJobTitles.AddRange(refereeJobTitlesResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeJobTitlesResult.Excptn, 
                this.ToString(), nameof(GetRefereeJobTitles));        
        }
    }
    
    /// <summary>
    /// Получение (обновление) коллекции судей.
    /// </summary>
    private async Task GetRefereesAsync()
    {
        var refereesResult = await _refereeService.GetRefereesAsync(Referees, CurrentCompetition);
        if (! refereesResult)
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereesResult.Excptn, this.ToString(), nameof(GetRefereesAsync));        
        }
    }

    /// <summary>
    /// Создание судьи.
    /// </summary>
    private async Task CreateRefereeAsync()
    {
        var refereeResult = await _refereeService.CreateRefereeAsync(
            Referees, Referees.SelectedIndex, CurrentCompetition);
        if (refereeResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereeResult.Value;
            
            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Добавили судью.", BaseException.ExcptnType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeResult.Excptn, this.ToString(), nameof(CreateRefereeAsync));        
        }
    }

    /// <summary>
    /// Удаление судьи.
    /// </summary>
    private void RemoveReferee()
    {
        var refereeResult = _refereeService.RemoveReferee(Referees, Referees.SelectedIndex);
        if (refereeResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereeResult.Value;
            
            // TODO: Временно (без ожидания окончания)
            if (refereeResult.Value >= 0)
                _ = _statusBarService.SetTextAsync("Удалили судью.", BaseException.ExcptnType.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeResult.Excptn, this.ToString(), nameof(RemoveReferee));        
        }
    }

    /// <summary>
    /// Перенумерация коллекции судей.
    /// </summary>
    private void RenumberReferee()
    {
        var index = Referees.SelectedIndex;
        _refereeService.RenumberReferees(Referees);
        Referees.SelectedIndex = index;
    }
    
    #endregion

}
