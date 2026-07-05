using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Cogs.Collections;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
using Common.WpfModule.Components.Wrappers;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Messages;
using Presentation.ViewModels.Common.Observables;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.Entities.LibraryEntities.Enums;
using Serilog;

namespace Presentation.ViewModels.MainView;

/// <summary>
/// ViewModel для меню ленты "Настройки".
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class SettingVM : ObservableRecipient, 
    IRecipient<LocalizationMessage>, IRecipient<AllCompetitionsMessage>, IStatusBarDataProvider, IDisposable
{
    private readonly LocalizationHelper _localizationHelper = null!;
    private readonly IViewWithResources _view = null!;
    private readonly ILogger _logger = null!;
    private readonly AppSettingService _appSetting = null!;
    private readonly CompetitionDataService _competitionDataService = null!;
    private readonly RefereeService _refereeService = null!;
    private readonly SportEventService _sportEventService = null!;

    /// <summary>
    /// Коллекция доступных языков.
    /// </summary>
    public ObservableCollection<Lang> Languages { get; } = null!;

    private Lang? _currLang;
    /// <summary>
    /// Текущая локализация.
    /// </summary>
    public Lang? CurrLang
    {
        get => _currLang;
        set
        {
            var oldLang = _currLang?.Clone() ?? _appSetting.AppLocalization.GetDefaultLang();
            if (SetProperty(ref _currLang, value))
            {
                // Оповещаем все представления (окна) приложения о смене локализации
                Messenger.Send(new LocalizationMessage(value ?? _appSetting.AppLocalization.GetDefaultLang(), oldLang ));
            }
        }
    }
    
    /// <summary>
    /// Сервис статус-бара.
    /// </summary>
    public StatusBarService StatusBarService { get; } = null!;
    
    
        
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
    public SettingVM(IViewWithResources view, 
        StatusBarService statusBarService, 
        ILogger logger, 
        IExceptionsProvider exceptionsProvider,
        AppSettingService appSetting,
        CompetitionDataService competitionDataService,
        RefereeService refereeService,
        SportEventService  sportEventService)
    {
        _view = view;
        StatusBarService = statusBarService;
        _logger = logger;
        _appSetting = appSetting;
        _competitionDataService = competitionDataService;
        _refereeService = refereeService;
        _sportEventService =  sportEventService;
        var localization = appSetting.AppLocalization;

        // Соревнования
        CreateConductingOrganizationCommand = new AsyncRelayCommand(CreateConductingOrganization);
        RemoveConductingOrganizationCommand = new AsyncRelayCommand(RemoveConductingOrganization);
        GetCompetitionCommand = new AsyncRelayCommand<int>(
            GetCompetitionDataAsync,
            id => id > 0 // CanExecute: только если Id корректный);
        );
        SaveCompetitionCommand = new AsyncRelayCommand(SaveCompetitionAsync);
        
        // Виды программы
        GetSportEventsCommand = new AsyncRelayCommand(GetSportEventObservablesAsync);
        CreateSportEventCommand = new AsyncRelayCommand(CreateSportEventAsync);
        RemoveSportEventCommand =  new AsyncRelayCommand(RemoveSportEvent);
        
        // Судьи
        GetRefereesCommand = new AsyncRelayCommand(GetRefereesAsync);
        CreateRefereeCommand = new AsyncRelayCommand(CreateRefereeAsync);
        RemoveRefereeCommand = new RelayCommand(RemoveReferee);
        RenumberRefereesCommand = new RelayCommand(RenumberReferee);
        
        // Установка локализации из настроек
        Languages = new ObservableCollection<Lang>(localization.Languages.Values);
        var langName = localization.GetLangFromSetting();
        _currLang = localization.SetCurrentLangFromName(langName);
        _localizationHelper = new LocalizationHelper(appSetting);

        // Подписываемся на получение сообщений
        Messenger.Register<LocalizationMessage>(this);
        Messenger.Register<AllCompetitionsMessage>(this);
        
        // Посылаем сообщение о смене локализации
        Messenger.Send( new LocalizationMessage(_currLang, localization.GetDefaultLang()));
        
        // Для отображения сообщений в нужной локализации при инициализации
        AppPhrases.Culture = _currLang.GetCultureInfo();

        // Обработка исключений "сверху", запуск инициализации если исключений нет
        ViewModelHelper.HandleExceptionsProvider(
            exceptionsProvider, InitAsync,
            StatusBarService, _logger);
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    public async void Receive(LocalizationMessage message)
    {
        try
        {
            var localization = _appSetting.AppLocalization;
        
            var lang = message.Lang;       // устанавливаемый язык
            var oldLang = message.OldLang; // предыдущий язык
        
            // Перевод наименований всех доступных языков приложения в соответствии с устанавливаемым языком
            localization.Translate(localization.SetCurrentLang(lang).GetCultureInfo());

            // Локализация представления асинхронно в UI-потоке, но без блокировки
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (! _localizationHelper.LocalizeView(_view, lang))
                {
                    // Если локализовать не получилось - переводим доступные языки обратно,
                    // в соответствии с предыдущим языком
                    localization.Translate(localization.SetCurrentLang(oldLang).GetCultureInfo());
                }
            }, DispatcherPriority.Background);
        
            // После завершения локализации обновляем культуру, UI, и фразы приложения
            var currLang = localization.GetCurrentOrDefaultLang();
            CultureInfo.CurrentUICulture = currLang.GetCultureInfo();
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
            AppPhrases.Culture = CultureInfo.CurrentUICulture;
        
            // Изменяем свойства, используемые для биндинга
            SetProperty(ref _currLang!, null);                          // отвязываем привязку свойства к событию изменения значения 
            Languages.Clear();
            localization.Languages.ForEach(item => Languages.Add(item.Value));
            SetProperty(ref _currLang, currLang, nameof(CurrLang));     // вновь привязываем
        
            // Дополнительные обновления данных
            await GetDisciplineGroupsAsync();
            await GetDisciplineSubGroupsAsync();
        }
        catch (Exception ex)
        {
            // Пишем в статус-бар и лог об ошибке
            var exception = new AppException(AppPhrases.LocalizingError.Format(nameof(SettingVM)), ex);
            _ = StatusBarService.SetTextAsync(exception.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(exception,
                "{class}.{method}.",
                typeof(SettingVM), nameof(Receive));
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
        // TODO: Возможно нужно сделать проверку результатов вызовов, и если false - делать выход
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
            _appSetting.SaveConfig();
        }
    }
    
    /// <inheritdoc />
    ~SettingVM() => Dispose(false);


    #region [---------- Данные о соревнованиях ----------]
        
    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    public ObservableCollection<CompetitionData> Competitions { get; set; } = [];

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
        // Сохраняем проводящие организации
        _competitionDataService.SetConductingOrganizations(ConductingOrganizations, CurrentCompetition);
        
        // Сохраняем изменения
        var intResult = await _competitionDataService.SaveCompetitionDataAsync();
        if (intResult)
        {
            // TODO: возможно изменим - Обновляем соревнование, хотя бы потому, чтобы обновилась коллекция сорев, при изменении ShortNeme одного из них
            if (CurrentCompetition != null) 
                // await GetCompetitionDataAsync(CurrentCompetition.Id);
                CurrentCompetition = CurrentCompetition;

            // Уведомляем UI об изменении
            // OnPropertyChanged(nameof(CurrentCompetition));
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(intResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(intResult.Excptn,
                "{class}.{method}.",
                typeof(SettingVM), nameof(SaveCompetitionAsync));
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
            _ = StatusBarService.SetTextAsync(detailedCompetitionsStatusesResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);            
            _logger.Error(detailedCompetitionsStatusesResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(GetDetailedCompetitionStatusesAsync));
        }
    }

    /// <summary>
    /// Создание проводящей организации.
    /// </summary>
    private Task CreateConductingOrganization()
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
            _ = StatusBarService.SetTextAsync(newIndexResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(newIndexResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(CreateConductingOrganization));
        }

        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Удаление проводящей организации.
    /// </summary>
    private Task RemoveConductingOrganization()
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
            _ = StatusBarService.SetTextAsync(newIndexResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(newIndexResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(RemoveConductingOrganization));
        }

        return Task.CompletedTask;
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
            if (exception != null)
            {
                // Пишем в статус-бар и лог об ошибке
                _ = StatusBarService.SetTextAsync(exception.Message,
                    BaseException.ExcptnType.Error, 0);
                _logger.Error(exception,
                    "{class}.{method}.",
                    typeof(SettingVM), nameof(GetCompetitionDataAsync));
            }
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
    public DisciplineGroup? FilteringDisciplineGroup =>
        DisciplineGroupsWithNull.SelectedIndex >= 0
            ? DisciplineGroupsWithNull[DisciplineGroupsWithNull.SelectedIndex].Key
            : null;
    
    /// <summary>
    /// Подгруппа спортивных дисциплин, ограничивающая выбор возможных дисциплин.
    /// </summary>
    public DisciplineSubGroup? FilteringDisciplineSubGroup =>
        DisciplineSubGroupsWithNull.SelectedIndex >= 0
            ? DisciplineSubGroupsWithNull[DisciplineSubGroupsWithNull.SelectedIndex].Key
            : null;

    /// <summary>
    /// Коллекция дисциплин.
    /// </summary>
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
            _ = StatusBarService.SetTextAsync(difficultiesResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(difficultiesResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetSportEventObservablesAsync));
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
            _ = StatusBarService.SetTextAsync("Добавили вид программы.", BaseException.ExcptnType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(sportEventResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);
            _logger.Error(sportEventResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(CreateSportEventAsync));
        }
    }

    /// <summary>
    /// Удаление Observable-вида программы.
    /// </summary>
    private Task RemoveSportEvent()
    {
        // Индекс
        var index = SportEventObservables.SelectedIndex;
        if (index < 0)
            return Task.CompletedTask;
        
        // Удаляем из репозитория
        var intResult = _sportEventService.RemoveSportEvent(SportEventObservables[index].SportEvent);
        if (intResult)
        {

            // Удаляем из коллекции
            SportEventObservables.RemoveAt(index);

            // Обновляем индекс
            SportEventObservables.SelectedIndex = index == 0 ? 0 : --index;

            return Task.CompletedTask;
        }
        
        // Пишем в статус-бар и лог об ошибке
        _ = StatusBarService.SetTextAsync(intResult.Excptn?.Message, 
            BaseException.ExcptnType.Error, 0);
        _logger.Error(intResult.Excptn, "{class}.{method}", 
            typeof(SettingVM), nameof(RemoveSportEvent));
        
        return Task.CompletedTask;
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
            _ = StatusBarService.SetTextAsync(disciplineGroupsResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(disciplineGroupsResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetDisciplineGroupsAsync));
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
            _ = StatusBarService.SetTextAsync(disciplineSubGroupsResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(disciplineSubGroupsResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetDisciplineSubGroupsAsync));
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
            _ = StatusBarService.SetTextAsync(disciplinesResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(disciplinesResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetDisciplinesAsync));
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
            _ = StatusBarService.SetTextAsync(difficultiesResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(difficultiesResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetDifficultiesDicAsync));
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
            _ = StatusBarService.SetTextAsync(refereeLevelsResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);            
            _logger.Error(refereeLevelsResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(GetRefereeLevels));
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
            _ = StatusBarService.SetTextAsync(refereeJobTitlesResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);            
            _logger.Error(refereeJobTitlesResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(GetRefereeJobTitles));
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
            _ = StatusBarService.SetTextAsync(refereesResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(refereesResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(GetRefereesAsync));
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
            _ = StatusBarService.SetTextAsync("Добавили судью.", BaseException.ExcptnType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(refereeResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);
            _logger.Error(refereeResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(CreateRefereeAsync));
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
                _ = StatusBarService.SetTextAsync("Удалили судью.", BaseException.ExcptnType.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(refereeResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);            
            _logger.Error(refereeResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(RemoveReferee));
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
