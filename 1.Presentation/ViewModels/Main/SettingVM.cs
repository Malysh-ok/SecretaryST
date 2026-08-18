using System.Collections.ObjectModel;
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
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
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
    private readonly AppSettingsService _appSettingsService = null!;
    private readonly StatusBarService _statusBarService = null!;
    private readonly CompetitionDataService _competitionDataService = null!;
    private readonly RefereeService _refereeService = null!;
    private readonly SportEventService _sportEventService = null!;
    private readonly ViewLocalizationService _viewLocalizationService = null!;
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
        AppSettingsService appSettingsService,
        StatusBarService statusBarService,
        CompetitionDataService competitionDataService,
        RefereeService refereeService,
        SportEventService sportEventService)
    {
        _view = view;
        _appErrorMsgProvider = appErrorMsgProvider;
        _appSettingsService = appSettingsService;
        _statusBarService = statusBarService;
        _competitionDataService = competitionDataService;
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
        GetSportEventsCommand = new AsyncRelayCommand(GetSportEventObservablesAsync);
        CreateSportEventCommand = new AsyncRelayCommand(CreateSportEventAsync);
        RemoveSportEventCommand =  new RelayCommand(RemoveSportEvent);
        RenumberSportEventsCommand = new RelayCommand(RenumberSportEvents);

        // Судьи
        GetRefereesCommand = new AsyncRelayCommand(GetRefereesAsync);
        CreateRefereeCommand = new AsyncRelayCommand(CreateRefereeAsync);
        RemoveRefereeCommand = new RelayCommand(RemoveReferee);
        RenumberRefereesCommand = new RelayCommand(RenumberReferees);

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
        await GetRefereeLevelsAsync();
        await GetRefereeJobTitlesAsync();
        await GetDisciplineGroupsAsync();
        await GetDisciplineSubGroupsAsync();
        await GetDisciplinesAsync();
        await GetDifficultiesAsync();
        await GetAgeGroupsAsync();
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
            _appSettingsService.SaveConfig();
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
                _ = GetCompetitionAsync(value.Id);

                IsStudentCompetition = value.IsStudentCompetition;
                    
                // Посылаем сообщение об изменении текущего соревнования
                Messenger.Send(new CompetitionMessage(CurrentCompetition));
            }
        }
    }

    /// <summary>
    /// Признак того, что соревнования студенческие.
    /// </summary>
    public bool IsStudentCompetition
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CurrentCompetition?.IsStudentCompetition = value;
                
                // Обновляем список доступных возрастных групп во всей коллекции
                SportEventObservables.ForEach(seo => seo.UpdateAvailableAgeGroups());
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

    /// <summary>
    /// Команда перенумеровывания проводящих организаций.
    /// </summary>
    public ICommand RenumberConductingOrganizationsCommand { get; } = null!;
    
    #endregion
    
    /// <summary>
    /// Получение полных данных о соревновании.
    /// </summary>
    private async Task GetCompetitionAsync(int id)
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
            GetConductingOrganizations();

            // Обновляем список видов программы
            _ = GetSportEventObservablesAsync();

            // Обновляем список судей
            _ = GetRefereesAsync();
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
            var intResult = await _competitionDataService.SaveCompetitionDataAsync();
            if (! intResult)
            {
                exception = intResult.Excptn;
                return;
            }

            // TODO: возможно изменим - Обновляем соревнование, хотя бы потому, чтобы обновилась коллекция соревнований, при изменении ShortName одного из них
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
        var getOrganizationsResult = _competitionDataService.GetConductingOrganizations(CurrentCompetition);
        if (getOrganizationsResult)
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
            new ConductingOrganizationObservable(0, _competitionDataService.CreateConductingOrganization()));
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
            _competitionDataService.SetConductingOrganizations(CurrentCompetition, conductingOrganizationList);
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
    public ObservableCollectionEx<Discipline> AvailableDisciplines { get; } = [];

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
    
    /// <summary>
    /// Команда перенумеровывания видов программы.
    /// </summary>
    public ICommand RenumberSportEventsCommand { get; } = null!;

    #endregion

    /// <summary>
    /// Получение (обновление) коллекции Observable-видов программы.
    /// </summary>
    private async Task GetSportEventObservablesAsync()
    {
        // Получаем виды программы
        var sportEventsResult = await _sportEventService.GetSportEventsAsync(CurrentCompetition!);
        if (sportEventsResult)
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
                this.ToString(), nameof(GetSportEventObservablesAsync));
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

        var sportEventResult = await _sportEventService.CreateSportEventAsync(index, CurrentCompetition, AvailableDisciplines);
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
    /// Получение (обновление) коллекции групп дисциплин.
    /// </summary>
    private async Task GetDisciplineGroupsAsync()
    {
        var disciplineGroupsResult = await _sportEventService.GetDisciplineGroupsAsync();
        if (disciplineGroupsResult)
        {
            // Отписываемся от события изменения индекса коллекции
            DisciplineGroupsWithNull.SelectedIndexChanged -= OnSelectedDisciplineGroupChanged;
            
            // Сохраняем индекс
            var index = DisciplineGroupsWithNull.SelectedIndex;
            if (index < 0 || index > DisciplineGroupsWithNull.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            var newList = new List<KeyValuePair<DisciplineGroup?, string>>
            {
                new(null, _viewLocalizationService.GetLocalizedString(_view, "NullDisplayText"))
            };
            disciplineGroupsResult.Value.ForEach(item => newList.Add(
                new KeyValuePair<DisciplineGroup?, string>(item, item.ToString())));
            DisciplineGroupsWithNull.ClearAndAddRange(newList);

            // Устанавливаем индекс
            DisciplineGroupsWithNull.SelectedIndex = index;
            
            // Подписываемся на событие изменения индекса коллекции
            DisciplineGroupsWithNull.SelectedIndexChanged += OnSelectedDisciplineGroupChanged;
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
            // Отписываемся от события изменения индекса коллекции
            DisciplineSubGroupsWithNull.SelectedIndexChanged -= OnSelectedDisciplineSubGroupChanged;

            // Сохраняем индекс
            var index = DisciplineSubGroupsWithNull.SelectedIndex;
            if (index < 0 || index > DisciplineSubGroupsWithNull.Count)
                index = 0;

            // Перезаписываем коллекцию групп дисциплин с null
            var newList = new List<KeyValuePair<DisciplineSubGroup?, string>>
            {
                new(null, _viewLocalizationService.GetLocalizedString(_view, "NullDisplayText"))
            };
            disciplineSubGroupsResult.Value.ForEach(item => newList.Add(
                new KeyValuePair<DisciplineSubGroup?, string>(item, item.ToString())));
            DisciplineSubGroupsWithNull.ClearAndAddRange(newList);

            // Устанавливаем индекс
            DisciplineSubGroupsWithNull.SelectedIndex = index;
            
            // Подписываемся на событие изменения индекса коллекции
            DisciplineSubGroupsWithNull.SelectedIndexChanged += OnSelectedDisciplineSubGroupChanged;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(disciplineSubGroupsResult.Excptn, 
                this.ToString(), nameof(GetDisciplineSubGroupsAsync));        
        }
    }

    /// <summary>
    /// Обновление коллекции доступных дисциплин.
    /// </summary>
    private void UpdateAvailableDisciplines()
    {
        // Фильтруем дисциплины по ограничивающим подгруппе и группе дисциплин
        AvailableDisciplines.ClearAndAddRange(_sportEventService.GetAvailableDisciplines(
            Disciplines, FilteringDisciplineSubGroup, FilteringDisciplineGroup));
    }

    /// <summary>
    /// Обработчик изменения выбранной группы дисциплин.
    /// </summary>
    private void OnSelectedDisciplineGroupChanged(int selectedIndex)
    {
        if (selectedIndex > 0)
            // Сбрасываем выбранную подгруппу дисциплин
            DisciplineSubGroupsWithNull.SelectedIndex = 0;

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
            DisciplineGroupsWithNull.SelectedIndex = 0;
        
        // Обновляем коллекцию доступных дисциплин
        UpdateAvailableDisciplines();
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
    /// Перенумерация коллекции Observable-видов программы.
    /// </summary>
    private void RenumberSportEvents()
    {
        for (var i = 0; i < SportEventObservables.Count; i++)
        {
            SportEventObservables[i].Number = i + 1;
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
    /// Команда перенумеровывания судей.
    /// </summary>
    public ICommand RenumberRefereesCommand { get; } = null!;

    #endregion

    /// <summary>
    /// Получение судейских категорий.
    /// </summary>
    private async Task GetRefereeLevelsAsync()
    {
        var refereeLevelsResult = await _refereeService.GetRefereeLevelsAsync();
        if (refereeLevelsResult)
        {
            // Перезаписываем коллекцию судейских категорий
            RefereeLevels.ClearAndAddRange(refereeLevelsResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeLevelsResult.Excptn, 
                this.ToString(), nameof(GetRefereeLevelsAsync));        
        }
    }

    /// <summary>
    /// Получение судейских должностей.
    /// </summary>
    private async Task GetRefereeJobTitlesAsync()
    {
        var refereeJobTitlesResult = await _refereeService.GetRefereeJobTitlesAsync();
        if (refereeJobTitlesResult)
        {
            // Перезаписываем коллекцию судейских должностей
            RefereeJobTitles.ClearAndAddRange(refereeJobTitlesResult.Value);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _viewModelHelper.HandleException(refereeJobTitlesResult.Excptn, 
                this.ToString(), nameof(GetRefereeJobTitlesAsync));        
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
        var refereeResult = _refereeService.RemoveReferee(Referees, Referees.SelectedIndex);
        if (refereeResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereeResult.Value;
            
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
        var index = Referees.SelectedIndex;
        _refereeService.RenumberReferees(Referees);
        Referees.SelectedIndex = index;
    }

    #endregion
}
