using System.Collections.Specialized;
using System.Windows.Input;
using AppDomain.AppEntities;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using AppDomain.AppUseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
using Common.WpfModule.Ui.Services;
using Common.WpfModule.Ui.Views._Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels.Shared.Infrastructure;
using Presentation.ViewModels.Shared.Infrastructure._Contracts;
using Presentation.ViewModels.Shared.Messages;
using Presentation.ViewModels.Shared.Models;
using Presentation.ViewModels.Shared.Models._Contracts;
using ProblemDomain.Entities.LibraryEntities;
using ProblemDomain.UseCases.Services;
using Serilog;

namespace Presentation.ViewModels.Main;

/// <summary>
/// ViewModel для меню ленты "Настройки".
/// </summary>
// ReSharper disable once InconsistentNaming
public sealed class SettingVM : ObservableRecipient, IRecipient<LocalizationMessage>, IRecipient<AllCompetitionsMessage>, 
    ICompetitionChangeNotifier, IRefereeChangeNotifier,
    IDisposable
{
    private readonly IViewWithResources _view;
    private readonly IAppErrorMsgProvider _appErrorMsgProvider;
    private readonly AppSettingsService _appSettingsService;
    private readonly StatusBarService _statusBarService;
    private readonly CompetitionService _competitionService;
    private readonly RefereeService _refereeService;
    private readonly SportEventService _sportEventService;
    private readonly ViewLocalizationService _viewLocalizationService;
    private readonly ViewModelHelper _viewModelHelper;

    /// <summary>
    /// Текущая локализация.
    /// </summary>
    public Lang? CurrLang
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public SettingVM(
        IViewWithResources view,
        ILogger logger,
        IExceptionsProvider exceptionsProvider,
        IAppErrorMsgProvider appErrorMsgProvider,
        AppSettingsService appSettingsService,
        StatusBarService statusBarService,
        CompetitionService competitionService,
        RefereeService refereeService,
        SportEventService sportEventService)
    {
        _view = view;
        _appErrorMsgProvider = appErrorMsgProvider;
        _appSettingsService = appSettingsService;
        _statusBarService = statusBarService;
        _competitionService = competitionService;
        _refereeService = refereeService;
        _sportEventService =  sportEventService;
        _viewLocalizationService = new ViewLocalizationService(appSettingsService);
        _viewModelHelper = new ViewModelHelper(logger, appErrorMsgProvider, statusBarService);

        // Соревнования
        CreateConductingOrganizationCommand = new RelayCommand(CreateConductingOrganization);
        RemoveConductingOrganizationCommand = new RelayCommand(RemoveConductingOrganization);
        RenumberConductingOrganizationsCommand = new RelayCommand(RenumberConductingOrganizations);
        GetCompetitionCommand = new AsyncRelayCommand<int>(
            GetCompetitionAsync,
            id => id > 0 // CanExecute: только если Id корректный);
        );
        SaveCompetitionCommand = new AsyncRelayCommand(SaveCompetitionAsync);

        // Виды программы
        UpdateSportEventsCommand = new AsyncRelayCommand(UpdateSportEventAsync);
        CreateSportEventCommand = new AsyncRelayCommand(CreateSportEventAsync);
        RemoveSportEventCommand =  new RelayCommand(RemoveSportEvent);
        RenumberSportEventsCommand = new RelayCommand(RenumberSportEvents);
        SportEventObservables.CollectionChanged += OnSportEventCollectionChanged;   //!!!!!!!!!!!!!!!

        // Судьи
        UpdateRefereesCommand = new AsyncRelayCommand(UpdateRefereesAsync);
        CreateRefereeCommand = new AsyncRelayCommand(CreateRefereeAsync);
        RemoveRefereeCommand = new RelayCommand(RemoveReferee);
        RenumberRefereesCommand = new RelayCommand(RenumberReferees);
        IsRefereeDataSelectionRestricted = true;        // TODO: Временная установка IsRefereeDataSelectionRestricted

        // Подписываемся на получение сообщений
        Messenger.Register<LocalizationMessage>(this);
        Messenger.Register<AllCompetitionsMessage>(this);

        // Получаем язык локализации из настроек и отправляем начальное сообщение
        // (по сути - только главному представлению)
        var initialLang = appSettingsService.AppLocalization.CurrentLang;
        Messenger.Send(new LocalizationMessage(initialLang, null));

        // Если при инициализации AppLocalization была ошибка - пишем о ней в лог
        if (! appSettingsService.AppLocalization.IsInitializedSuccessfully)
            _viewModelHelper.HandleException(appSettingsService.AppLocalization.InitializationException,
                this.ToString(), "ctor");

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
            // Если текущий язык равен переданному - ничего не делаем
            if (CurrLang == message.Lang)
                return;

            // Устанавливаем текущий язык
            CurrLang = message.Lang;

            if (! _viewLocalizationService.LocalizeView(_view, message.Lang))
            {
                // Если локализовать не получилось - возвращаем предыдущий язык
                CurrLang = message.OldLang;
            }

            // Дополнительные обновления данных (т.к. необходим перевод невыбранного значения)
            await GetDisciplineGroupsAsync();
            await GetDisciplineSubGroupsAsync();
            await GetDisciplineGroupsForRefereesAsync();
        }
        catch (Exception ex)
        {
            // Пишем в статус-бар и лог об ошибке
            var exception =_appErrorMsgProvider.CreateException(AppErrorCodes.LocalizingError, ex, args: this.GetType().Name);
            _viewModelHelper.HandleException(exception, this.ToString(), nameof(Receive));
        }
    }

    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="AllCompetitionsMessage"/>.
    /// </summary>
    public void Receive(AllCompetitionsMessage message)
    {
        Competitions.ClearAndAddRange(message.Competitions);
        CurrentCompetition = message.CurrentCompetition;
    }

    /// <summary>
    /// Инициализация - получение всех необходимых данных.
    /// </summary>
    private async Task InitAsync()
    {
        // TODO: Возможно нужно сделать проверку результатов вызовов, и если false - как-то реагировать
        await GetDetailedCompetitionStatusesAsync();
        await GetDisciplineGroupsAsync();
        await GetDisciplineSubGroupsAsync();
        await GetDisciplinesAsync();
        await GetDifficultiesAsync();
        await GetAgeGroupsAsync();
        await GetDisciplineGroupsForRefereesAsync();
        await GetRefereeCategoriesAsync();
        await GetRefereeRolesAsync();

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
            SportEventObservables.CollectionChanged -= OnSportEventCollectionChanged;
            _appSettingsService.SaveConfig();
        }
    }

    /// <inheritdoc />
    ~SettingVM() => Dispose(false);

    #region [---------- Данные о соревнованиях ----------]

    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    public ObservableCollectionEx<CompetitionObservable> Competitions { get; private set; } = [];

    private CompetitionObservable? _currentCompetition;
    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionObservable? CurrentCompetition
    {
        get => _currentCompetition;
        set
        {
            if (SetProperty(ref _currentCompetition, value))
            {
                // При смене выбранного соревнования перезагружаем его с навигационными свойствами (асинхронно "запустил и забыл")
                _ = GetCompetitionAsync(value?.Competition.Id ?? 0);
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
    public ObservableCollectionEx<ConductingOrganizationObservable> ConductingOrganizations { get; set; } = [];

    #region [---------- Команды ----------]

    /// <summary>
    /// Команда сохранения соревнования (включая зависимые сущности).
    /// </summary>
    public ICommand SaveCompetitionCommand { get; }

    /// <summary>
    /// Команда получения соревнования.
    /// </summary>
    public IAsyncRelayCommand<int> GetCompetitionCommand { get; }

    /// <summary>
    /// Команда создания проводящей организации.
    /// </summary>
    public ICommand CreateConductingOrganizationCommand { get; }

    /// <summary>
    /// Команда удаления проводящей организации.
    /// </summary>
    public ICommand RemoveConductingOrganizationCommand { get; }

    /// <summary>
    /// Команда перенумеровывания проводящих организаций.
    /// </summary>
    public ICommand RenumberConductingOrganizationsCommand { get; }
    
    #endregion
    
    /// <summary>
    /// Получение полных данных о соревновании.
    /// </summary>
    private async Task GetCompetitionAsync(int id)
    {
        Exception? exception = null;
        try
        {
            if (id > 0)
            {
                // Получаем
                var competitionResult = await _competitionService.GetCompetitionAsync(id, true);
                if (! competitionResult)
                {
                    // Неудачное получение данных из репозитория
                    exception = competitionResult.Excptn;
                    return;
                }

                // Заменяем объект в коллекции соревнований
                var competitionObservable = Competitions.FirstOrDefault(c => c.Competition.Id == id);
                if (competitionObservable != null)
                {
                    // Обновление
                    competitionObservable.UpdateFrom(_competitionService, competitionResult.Value!, this);
                }
                else
                {
                    // Добавление
                    competitionObservable = CompetitionObservable.CreateComplete(_competitionService,
                        competitionResult.Value!,
                        this);
                    Competitions.Add(competitionObservable);
                }

                // Заменяем текущие соревнования (меняем через поле, чтобы не вызвать данный метод повторно)
                SetProperty(ref _currentCompetition, competitionObservable, nameof(CurrentCompetition));
            }

            // Заполняем проводящие организации
            GetConductingOrganizations();

            // Обновляем список видов программы
            await UpdateSportEventAsync();

            // Обновляем список судей
            await UpdateRefereesAsync();
                            
            // Посылаем сообщение об изменении текущего соревнования
            Messenger.Send(new CompetitionMessage(CurrentCompetition));
        }
        finally
        {
            // Пишем в статус-бар и лог об ошибке при ее наличии
            _viewModelHelper.HandleException(exception, this.ToString(), nameof(GetCompetitionAsync));
        }
    }
        
    /// <summary>
    /// Сохранение соревнования (включая зависимые сущности).
    /// </summary>
    private async Task SaveCompetitionAsync()
    {
        Exception? exception = null;
        try
        {
            // Сохраняем проводящие организации
            SaveConductingOrganizations();

            // Сохраняем изменения
            var intResult = await _competitionService.SaveCompetitionAsync();
            if (! intResult)
            {
                exception = intResult.Excptn;
            }
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
        var detailedCompetitionsStatusesResult = await _competitionService.GetDetailedCompetitionsStatusesAsync();

        if (detailedCompetitionsStatusesResult)
        {
            // Перезаписываем коллекцию статусов и наименований соревнований
            DetailedCompetitionStatuses.ClearAndAddRange(detailedCompetitionsStatusesResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(detailedCompetitionsStatusesResult.Excptn, 
                this.ToString(), nameof(GetDetailedCompetitionStatusesAsync));
        }
    }

    /// <summary>
    /// Получение (обновление) коллекции Observable-проводящих организаций.
    /// </summary>
    private void GetConductingOrganizations()
    {
        // Получаем проводящие организации
        var getOrganizationsResult = _competitionService.GetConductingOrganizations(CurrentCompetition?.Competition);
        if (CurrentCompetition == null)
        {
            ConductingOrganizations.ClearAndAddRange([]);
        }
        else if (getOrganizationsResult)
        {
            var i = 1;
            var newList = new List<ConductingOrganizationObservable>();
            getOrganizationsResult.Value.ForEach(item =>
            {
                newList.Add(new ConductingOrganizationObservable(i++, item));
            });
            ConductingOrganizations.ClearAndAddRange(newList);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(getOrganizationsResult.Excptn, 
                this.ToString(), nameof(GetConductingOrganizations));
        }
    }

    /// <summary>
    /// Создание Observable-проводящей организации.
    /// </summary>
    private void CreateConductingOrganization()
    {
        var index = ConductingOrganizations.SelectedIndex;
        if (index < 0)
            index = ConductingOrganizations.Count;
        else
            index++;

        // Добавляем в коллекцию организацию и перенумеровываем коллекцию
        ConductingOrganizations.Insert(index, 
            new ConductingOrganizationObservable(0, _competitionService.CreateConductingOrganization()));
        RenumberConductingOrganizations();
        
        // Присваиваем новый индекс
        ConductingOrganizations.SelectedIndex = index;
    }

    /// <summary>
    /// Удаление Observable-проводящей организации.
    /// </summary>
    private void RemoveConductingOrganization()
    {
        // Индекс
        var index = ConductingOrganizations.SelectedIndex;
        if (index < 0)
            return;

        // Удаляем из коллекции организацию и перенумеровываем коллекцию
        ConductingOrganizations.RemoveAt(index);
        RenumberConductingOrganizations();
        
        // Присваиваем новый индекс
        ConductingOrganizations.SelectedIndex = index == ConductingOrganizations.Count ? index - 1 : index;
    }
    
    /// <summary>
    /// Сохранение коллекции Observable-проводящих организаций.
    /// </summary>
    private void SaveConductingOrganizations()
    {
        // Получаем список строк
        var conductingOrganizationList = new List<string>();
        ConductingOrganizations.ForEach(item => 
            conductingOrganizationList.Add(item.Name));
        
        // Сохраняем
        var saveOrganisationsResult = 
            _competitionService.SetConductingOrganizations(CurrentCompetition?.Competition, conductingOrganizationList);
        if (! saveOrganisationsResult)
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(saveOrganisationsResult.Excptn, 
                this.ToString(), nameof(SaveConductingOrganizations));
        }
    }

    /// <summary>
    /// Перенумерация коллекции Observable-проводящих организаций.
    /// </summary>
    private void RenumberConductingOrganizations()
    {
        for (var i = 0; i < ConductingOrganizations.Count; i++)
        {
            ConductingOrganizations[i].Number = i + 1;
        }
    }
    
    /// <inheritdoc/>
    public void OnCompetitionChanged(CompetitionObservable competitionObservable, string propertyName)
    {
        switch (propertyName)
        {
            case nameof(CompetitionObservable.IsStudentCompetition):
                // Обновляем список доступных возрастных групп во всей коллекции видов программы
                SportEventObservables.ForEach(seo => seo.UpdateAvailableAgeGroups());
                break;
            case nameof(CompetitionObservable.InitialDate):
                // Обновляем признаки просроченности категорий во всей коллекции судей
                RefereeObservables.ForEach(ro => ro.UpdateCategoryExpiration());
                break;
            case nameof(CompetitionObservable.DetailedCompetitionStatus):
                // Обновляем зависимые коллекции во всей коллекции судей (асинхронно "запустил и забыл")
                _ = RefereeObservables.ForEachAsync(async ro => await ro.UpdateDependentCollectionsAsync());
                break;
        }
    }

    #endregion

    #region [---------- Виды программы ----------]

    /// <summary>
    /// Коллекция трудностей вида программы.
    /// </summary>
    private IList<Difficulty> _difficulties = [];

    /// <summary>
    /// Коллекция возрастных групп.
    /// </summary>
    private IList<AgeGroup> _ageGroups = [];

    /// <summary>
    /// Коллекция Observable-видов программы.
    /// </summary>
    public ObservableCollectionEx<SportEventObservable> SportEventObservables { get; set; } = [];

    /// <summary>
    /// Коллекция групп дисциплин для видов программы (включая null).
    /// </summary>
    public ObservableCollectionEx<KeyValuePair<DisciplineGroup?, string>> DisciplineGroupsForSportEvents { get; } = [];

    /// <summary>
    /// Коллекция подгрупп дисциплин для видов программы (включая null).
    /// </summary>
    public ObservableCollectionEx<KeyValuePair<DisciplineSubGroup?, string>> DisciplineSubGroupsForSportEvents { get; } = [];

    /// <summary>
    /// Группа спортивных дисциплин, ограничивающая выбор возможных дисциплин для видов программы.
    /// </summary>
    private DisciplineGroup? DisciplineGroupFilterForSportEvents =>
        DisciplineGroupsForSportEvents.SelectedIndex >= 0
            ? DisciplineGroupsForSportEvents[DisciplineGroupsForSportEvents.SelectedIndex].Key
            : null;

    /// <summary>
    /// Подгруппа спортивных дисциплин, ограничивающая выбор возможных дисциплин для видов программы.
    /// </summary>
    private DisciplineSubGroup? DisciplineSubGroupFilterForSportEvents =>
        DisciplineSubGroupsForSportEvents.SelectedIndex >= 0
            ? DisciplineSubGroupsForSportEvents[DisciplineSubGroupsForSportEvents.SelectedIndex].Key
            : null;

    /// <summary>
    /// Коллекция дисциплин.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public ObservableCollectionEx<Discipline> Disciplines { get; } = [];

    /// <summary>
    /// Коллекция доступных для выбора дисциплин. 
    /// </summary>
    public ObservableCollectionEx<Discipline> AvailableDisciplines { get; } = [];

    #region [---------- Команды ----------]

    /// <summary>
    /// Команда получения (обновления) коллекции видов программы.
    /// </summary>
    public ICommand UpdateSportEventsCommand { get; }

    /// <summary>
    /// Команда создания вида программы.
    /// </summary>
    public ICommand CreateSportEventCommand { get; }

    /// <summary>
    /// Команда удаления вида программы.
    /// </summary>
    public ICommand RemoveSportEventCommand { get; }
    
    /// <summary>
    /// Команда перенумеровывания видов программы.
    /// </summary>
    public ICommand RenumberSportEventsCommand { get; }

    #endregion

    /// <summary>
    /// Получение (обновление) коллекции Observable-видов программы.
    /// </summary>
    private async Task UpdateSportEventAsync()
    {
        // Получаем виды программы
        var sportEventsResult = await _sportEventService.GetAllSportEventsAsync(CurrentCompetition?.Competition);
        if (CurrentCompetition == null)
        {
            SportEventObservables.ClearAndAddRange([]);
        }
        else if (sportEventsResult)
        {
            // Перезаписываем коллекцию
            var newList = new List<SportEventObservable>();
            sportEventsResult.Value.ForEach(item =>
            {
                var seo = new SportEventObservable(_sportEventService, _difficulties, _ageGroups, item, AvailableDisciplines);
                newList.Add(seo);
            });
            SportEventObservables.ClearAndAddRange(newList);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(sportEventsResult.Excptn, 
                this.ToString(), nameof(UpdateSportEventAsync));
        }
    }

    /// <summary>
    /// Создание Observable-вида программы.
    /// </summary>
    private async Task CreateSportEventAsync()
    {
        // Индекс
        var index = SportEventObservables.SelectedIndex;
        if (index < 0)
            index = SportEventObservables.Count;
        else
            index++;

        var sportEventResult = await _sportEventService.CreateSportEventAsync(index, CurrentCompetition?.Competition, AvailableDisciplines);
        if (sportEventResult)
        {
            // Добавляем в коллекцию Observable-вид программы и перенумеровываем коллекцию
            SportEventObservables.Insert(index, 
                new SportEventObservable(_sportEventService, _difficulties, _ageGroups, sportEventResult.Value!, AvailableDisciplines));
            RenumberSportEvents();

            // Присваиваем новый индекс
            SportEventObservables.SelectedIndex = index;

            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Добавили вид программы.", ExcptnTypeEnm.Info);
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
            // Удаляем из коллекции Observable-вид программы и перенумеровываем коллекцию
            SportEventObservables.RemoveAt(index);
            RenumberSportEvents();

            // Обновляем индекс
            SportEventObservables.SelectedIndex = index == SportEventObservables.Count ? index - 1 : index;

            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Удалили вид программы.", ExcptnTypeEnm.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(intResult.Excptn, this.ToString(), nameof(RemoveSportEvent));
        }
    }
    
    /// <summary>
    /// Перенумерация коллекции Observable-видов программы.
    /// </summary>
    private void RenumberSportEvents()
    {
        for (var i = 0; i < SportEventObservables.Count; i++)
        {
            SportEventObservables[i].Number = i + 1;
        }
    }

    /// <summary>
    /// Получение (обновление) коллекции групп дисциплин (для видов программы).
    /// </summary>
    private async Task GetDisciplineGroupsAsync()
    {
        var disciplineGroupsResult = await _sportEventService.GetAllDisciplineGroupsAsync();
        if (disciplineGroupsResult)
        {
            // Отписываемся от события изменения индекса коллекции
            DisciplineGroupsForSportEvents.SelectedIndexChanged -= OnSelectedDisciplineGroupChanged;
            
            // Сохраняем индекс
            var index = DisciplineGroupsForSportEvents.SelectedIndex;
            if (index < 0 || index > DisciplineGroupsForSportEvents.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            var newList = new List<KeyValuePair<DisciplineGroup?, string>>
            {
                new(null, _viewLocalizationService.GetLocalizedString(_view, "NullDisplayText"))
            };
            disciplineGroupsResult.Value.ForEach(item => newList.Add(
                new KeyValuePair<DisciplineGroup?, string>(item, item.ToString()))
            );
            DisciplineGroupsForSportEvents.ClearAndAddRange(newList);

            // Устанавливаем индекс
            DisciplineGroupsForSportEvents.SelectedIndex = index;
            
            // Подписываемся на событие изменения индекса коллекции
            DisciplineGroupsForSportEvents.SelectedIndexChanged += OnSelectedDisciplineGroupChanged;
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
        var disciplineSubGroupsResult = await _sportEventService.GetAllDisciplineSubGroupsAsync();
        if (disciplineSubGroupsResult)
        {
            // Отписываемся от события изменения индекса коллекции
            DisciplineSubGroupsForSportEvents.SelectedIndexChanged -= OnSelectedDisciplineSubGroupChanged;

            // Сохраняем индекс
            var index = DisciplineSubGroupsForSportEvents.SelectedIndex;
            if (index < 0 || index > DisciplineSubGroupsForSportEvents.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            var newList = new List<KeyValuePair<DisciplineSubGroup?, string>>
            {
                new(null, _viewLocalizationService.GetLocalizedString(_view, "NullDisplayText"))
            };
            disciplineSubGroupsResult.Value.ForEach(item => newList.Add(
                new KeyValuePair<DisciplineSubGroup?, string>(item, item.ToString()))
            );
            DisciplineSubGroupsForSportEvents.ClearAndAddRange(newList);

            // Устанавливаем индекс
            DisciplineSubGroupsForSportEvents.SelectedIndex = index;
            
            // Подписываемся на событие изменения индекса коллекции
            DisciplineSubGroupsForSportEvents.SelectedIndexChanged += OnSelectedDisciplineSubGroupChanged;
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
        var disciplinesResult = await _sportEventService.GetAllDisciplinesAsync();
        if (disciplinesResult)
        {
            // Перезаписываем коллекцию дисциплин
            Disciplines.ClearAndAddRange(disciplinesResult.Value);
            
            // Обновляем доступные дисциплины
            UpdateAvailableDisciplines();
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(disciplinesResult.Excptn, this.ToString(), nameof(GetDisciplinesAsync));        
        }
    }

    /// <summary>
    /// Получение (создание) коллекции трудностей видов программы.
    /// </summary>
    private async Task GetDifficultiesAsync()
    {
        var difficultiesResult = await _sportEventService.GetAllDifficultiesAsync();
        if (difficultiesResult)
        {
            // Создаем коллекцию
            _difficulties = difficultiesResult.Value!;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(difficultiesResult.Excptn, 
                this.ToString(), nameof(GetDifficultiesAsync));        
        }
    }

    /// <summary>
    /// Получение (создание) коллекции возрастных групп.
    /// </summary>
    private async Task GetAgeGroupsAsync()
    {
        var ageGroupsResult = await _sportEventService.GetAllAgeGroupsAsync();
        if (ageGroupsResult)
        {
            // Создаем коллекцию
            _ageGroups = ageGroupsResult.Value!;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(ageGroupsResult.Excptn, 
                this.ToString(), nameof(GetAgeGroupsAsync));        
        }
    }

    /// <summary>
    /// Обновление коллекции доступных дисциплин.
    /// </summary>
    private void UpdateAvailableDisciplines()
    {
        // Фильтруем дисциплины по ограничивающим подгруппе и группе дисциплин
        AvailableDisciplines.ClearAndAddRange(_sportEventService.GetAvailableDisciplines(
            Disciplines, DisciplineSubGroupFilterForSportEvents, DisciplineGroupFilterForSportEvents));
    }

    /// <summary>
    /// Обработчик изменения выбранной группы дисциплин.
    /// </summary>
    private void OnSelectedDisciplineGroupChanged(int selectedIndex)
    {
        if (selectedIndex > 0)
            // Сбрасываем выбранную подгруппу дисциплин
            DisciplineSubGroupsForSportEvents.SelectedIndex = 0;

        // Обновляем коллекцию доступных дисциплин
        UpdateAvailableDisciplines();
    }
    
    /// <summary>
    /// Обработчик изменения выбранной подгруппы дисциплин.
    /// </summary>
    private void OnSelectedDisciplineSubGroupChanged(int selectedIndex)
    {
        if (selectedIndex > 0)
            // Сбрасываем выбранную группу дисциплин
            DisciplineGroupsForSportEvents.SelectedIndex = 0;
        
        // Обновляем коллекцию доступных дисциплин
        UpdateAvailableDisciplines();
    }
    
    // !!!!!!!!!!!!!!!!!!!!!!!!
    private void OnSportEventCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Console.WriteLine(@$"====================== Изменение коллекции {nameof(SportEventObservables)}!");
    }

    #endregion

    #region [---------- Судьи ----------]

    /// <summary>
    /// Коллекция судейских категорий.
    /// </summary>
    private IList<RefereeCategory> _refereeCategories = [];

    /// <summary>
    /// Коллекция должностей.
    /// </summary>
    private IList<RefereeRole> _refereeRoles = [];

    /// <summary>
    /// Коллекция Observable-судей.
    /// </summary>
    public ObservableCollectionEx<RefereeObservable> RefereeObservables { get; set; } = [];

    /// <summary>
    /// Признак ограничения выбора данных у судей, в соответствии с бизнес-логикой.
    /// </summary>
    public bool IsRefereeDataSelectionRestricted
    {
        get; 
        set
        {
            if (SetProperty(ref field, value))
            {
                // Оповещаем ViewModel об изменении всех зависимых свойств
                OnRefereeChanged();
            }
        }
    }
    
    /// <summary>
    /// Коллекция групп дисциплин для судей (включая null).
    /// </summary>
    public ObservableCollectionEx<KeyValuePair<DisciplineGroup?, string>> DisciplineGroupsForReferees { get; } = [];
    
    /// <summary>
    /// Группа спортивных дисциплин, ограничивающая выбор возможных дисциплин для судей.
    /// </summary>
    /*
    public DisciplineGroup? DisciplineGroupFilterForReferees =>
        DisciplineGroupsForReferees.SelectedIndex >= 0
            ? DisciplineGroupsForReferees[DisciplineGroupsForReferees.SelectedIndex].Key
            : null;
    */
    public KeyValuePair<DisciplineGroup?, string> DisciplineGroupFilterForReferees
    {
        get; 
        set
        {
            if (SetProperty(ref field, value))
            {
                // Оповещаем ViewModel об изменении всех зависимых свойств
                OnRefereeChanged();
            }
        }
    }

    #region [---------- Команды ----------]

    /// <summary>
    /// Команда получения (обновления) списка судей.
    /// </summary>
    public ICommand UpdateRefereesCommand { get; }

    /// <summary>
    /// Команда создания судьи.
    /// </summary>
    public ICommand CreateRefereeCommand { get; }

    /// <summary>
    /// Команда удаления судьи.
    /// </summary>
    public ICommand RemoveRefereeCommand { get; }

    /// <summary>
    /// Команда перенумеровывания судей.
    /// </summary>
    public ICommand RenumberRefereesCommand { get; }

    #endregion

    /// <summary>
    /// Получение (обновление) коллекции судей.
    /// </summary>
    private async Task UpdateRefereesAsync()
    {
        // Получаем судей
        var refereesResult = await _refereeService.GetAllRefereesAsync(CurrentCompetition?.Competition);
        if (CurrentCompetition == null)
        {
            RefereeObservables.ClearAndAddRange([]);
        }
        else if (refereesResult)
        {
            // Перезаписываем коллекцию
            var newList = new List<RefereeObservable>();
            await refereesResult.Value.ForEachAsync(async item =>
            {
                var ro = await RefereeObservable.CreateAsync(
                    _viewModelHelper,
                    _refereeService, 
                    item, 
                    _refereeCategories, 
                    _refereeRoles,
                    this,
                    () => IsRefereeDataSelectionRestricted,
                    () => DisciplineGroupFilterForReferees.Key,
                    () => CurrentCompetition?.InitialDate,
                    () => SportEventObservables.Count,
                    GetOccupiedRefereeRoles);
                newList.Add(ro);
            });
            RefereeObservables.ClearAndAddRange(newList);
            
            // Оповещаем ViewModel об изменении всех зависимых свойств
            OnRefereeChanged();
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereesResult.Excptn, this.ToString(), nameof(UpdateRefereesAsync));
        }
    }

    /// <summary>
    /// Создание судьи.
    /// </summary>
    private async Task CreateRefereeAsync()
    {
        // Индекс
        var index = RefereeObservables.SelectedIndex;
        if (index < 0)
            index = RefereeObservables.Count;
        else
            index++;

        var refereeResult = await _refereeService.CreateRefereeAsync(index, CurrentCompetition?.Competition);
        if (refereeResult)
        {
            // Добавляем в коллекцию Observable-судью и перенумеровываем коллекцию
            var ro = await RefereeObservable.CreateAsync(
                _viewModelHelper,
                _refereeService, 
                refereeResult.Value!, 
                _refereeCategories, 
                _refereeRoles,
                this,
                () => IsRefereeDataSelectionRestricted,
                () => DisciplineGroupFilterForReferees.Key,
                () => CurrentCompetition?.InitialDate,
                () => SportEventObservables.Count,
                GetOccupiedRefereeRoles);
            RefereeObservables.Insert(index, ro);
            RenumberReferees();

            // Перезаписываем индекс
            RefereeObservables.SelectedIndex = index;
            
            // Оповещаем ViewModel об изменении всех зависимых свойств
            OnRefereeChanged();
            
            // TODO: Временно (без ожидания окончания)
            _ = _statusBarService.SetTextAsync("Добавили судью.", ExcptnTypeEnm.Info);
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
        // Индекс
        var index = RefereeObservables.SelectedIndex;
        if (index < 0)
            return;

        // Удаляем из репозитория
        var refereeResult = _refereeService.RemoveReferee(RefereeObservables[index].Referee);
        if (refereeResult)
        {
            // Удаляем из коллекции Observable-судью и перенумеровываем коллекцию
            RefereeObservables.RemoveAt(index);
            RenumberReferees();

            // Обновляем индекс
            RefereeObservables.SelectedIndex = index == RefereeObservables.Count ? index - 1 : index;
            
            // TODO: Временно (без ожидания окончания)
            if (refereeResult.Value >= 0)
                _ = _statusBarService.SetTextAsync("Удалили судью.", ExcptnTypeEnm.Error);
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
    private void RenumberReferees()
    {
        for (var i = 0; i < RefereeObservables.Count; i++)
        {
            RefereeObservables[i].Number = i + 1;
        }
    }

    /// <summary>
    /// Получение (обновление) коллекции групп дисциплин (для судей).
    /// </summary>
    private async Task GetDisciplineGroupsForRefereesAsync()
    {
        // Получаем через сервис видов программы
        var disciplineGroupsResult = await _sportEventService.GetAllDisciplineGroupsAsync();
        if (disciplineGroupsResult)
        {
            // Сохраняем индекс
            var index = DisciplineGroupsForReferees.SelectedIndex;
            if (index < 0 || index > DisciplineGroupsForReferees.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            var newList = new List<KeyValuePair<DisciplineGroup?, string>>
            {
                new(null, _viewLocalizationService.GetLocalizedString(_view, "NullDisplayText"))
            };
            disciplineGroupsResult.Value.ForEach(item => newList.Add(
                new KeyValuePair<DisciplineGroup?, string>(item, item.ToString()))
            );
            DisciplineGroupsForReferees.ClearAndAddRange(newList);

            // Устанавливаем индекс
            DisciplineGroupsForReferees.SelectedIndex = index;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(disciplineGroupsResult.Excptn, 
                this.ToString(), nameof(GetDisciplineGroupsForRefereesAsync));        
        }
    }
    
    /// <summary>
    /// Получение судейских категорий.
    /// </summary>
    private async Task GetRefereeCategoriesAsync()
    {
        var refereeCategoriesResult = await _refereeService.GetAllRefereeCategoriesAsync();
        if (refereeCategoriesResult)
        {
            // Перезаписываем коллекцию судейских категорий
            _refereeCategories = refereeCategoriesResult.Value!;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeCategoriesResult.Excptn, 
                this.ToString(), nameof(GetRefereeCategoriesAsync));        
        }
    }

    /// <summary>
    /// Получение судейских должностей.
    /// </summary>
    private async Task GetRefereeRolesAsync()
    {
        var refereeRolesResult = await _refereeService.GetAllRefereeRolesAsync();
        if (refereeRolesResult)
        {
            // Перезаписываем коллекцию судейских должностей
            _refereeRoles = refereeRolesResult.Value!;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeRolesResult.Excptn, 
                this.ToString(), nameof(GetRefereeRolesAsync));        
        }
    }
    
    /// <summary>
    /// Получение коллекции уже используемых судейских должностей.
    /// </summary>
    private IList<RefereeRole> GetOccupiedRefereeRoles() =>
        RefereeObservables.Select(ro => ro.RoleObservable.Role).ToList();
    
    /// <inheritdoc/>
    public void OnRefereeChanged(RefereeObservable? refereeObservable = null, string? propertyName = null)
    {
        switch (propertyName)
        {
            case nameof(RefereeObservable.RoleObservable):
                // Обновляем доступные судейские должности во всей коллекции судей
                _ = RefereeObservables.ForEachAsync(async ro => await ro.UpdateAvailableRolesAsync());
                break;
            
            default:
                // Обновляем все зависимые коллекции во всей коллекции судей
                _ = RefereeObservables.ForEachAsync(async ro => await ro.UpdateDependentCollectionsAsync());
                break;
        }
    }
    
    // !!!!!!!!!!!!!!!!!!!!!!!!
    private void OnRefereeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Console.WriteLine(@$"====================== Изменение коллекции {nameof(RefereeObservables)}!");    //!!!!!!!!!!!!!!
        
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                // Добавленный объект (один или несколько)
                if (e.NewItems?[0] is RefereeObservable addedReferee)
                {
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                // Удалённый объект (один или несколько)
                if (e.OldItems?[0] is RefereeObservable removedReferee)
                {
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                // Старый объект (который заменяют)
                if (e.OldItems?[0] is RefereeObservable oldReferee)
                {
                }
                // Новый объект (на который заменяют)
                if (e.NewItems?[0] is RefereeObservable newReferee)
                {
                }
                break;
            case NotifyCollectionChangedAction.Reset:
                // Вся коллекция очищена
                break;
        }
    }

    #endregion
}
