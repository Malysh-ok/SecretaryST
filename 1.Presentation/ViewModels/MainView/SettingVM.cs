using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AppDomain.AppExceptions;
using AppDomain.Phrases;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseComponents.Wrappers;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
using Common.WpfModule.Components.Services;
using Common.WpfModule.Ui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Presentation.ViewModels._Contracts;
using Presentation.ViewModels.Common;
using ProblemDomain.Entities.CommonEntities;
using ProblemDomain.Entities.LibraryEntities;
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

    #region [---------- Данные о соревнованиях ----------]

    /// <summary>
    /// Коллекция статусов и обобщенных наименований соревнований.
    /// </summary>
    public ObservableCollectionEx<DetailedCompetitionStatus> DetailedCompetitionStatuses { get; set; } = [];

    /// <summary>
    /// Коллекция проводящих организаций.
    /// </summary>
    public ObservableCollectionEx<StringItem> ConductingOrganizations { get; set; } = [];
        
    /// <summary>
    /// Коллекция соревнований.
    /// </summary>
    public ObservableCollection<CompetitionData> CompetitionDataCollection { get; set; } = [];

    private CompetitionData? _currentCompetitionData;
    /// <summary>
    /// Данные о текущем соревновании.
    /// </summary>
    public CompetitionData? CurrentCompetitionData
    {
        get => _currentCompetitionData;
        set
        {
            if (SetProperty(ref _currentCompetitionData, value) && value != null)
            {
                // При смене выбранного соревнования подгружаем навигационные свойства
                _ = OnGetCompetitionDataAsync(value.Id);
                
                if (CurrentCompetitionData != null) 
                    // Посылаем сообщение об изменении текущего соревнования
                    Messenger.Send(new CompetitionMessage(CurrentCompetitionData));
            }
        }
    }

    #endregion

    #region [---------- Судьи ----------]

    /// <summary>
    /// Список судейских категорий.
    /// </summary>
    public ObservableCollectionEx<RefereeLevel> RefereeLevels { get; } = [];
    
    /// <summary>
    /// Список должностей.
    /// </summary>
    public ObservableCollectionEx<RefereeJobTitle> RefereeJobTitles { get; } = [];

    /// <summary>
    /// Список судей.
    /// </summary>
    public ObservableCollectionEx<Referee> Referees { get; set; } = [];

    #endregion
    

    #region [---------- Команды ― Данные о соревн. ----------]
    
    /// <summary>
    /// Команда получения соревнования.
    /// </summary>
    public IAsyncRelayCommand<int> GetCompetitionCommand { get; } = null!;
    
    /// <summary>
    /// Команда сохранения изменений, связанных с соревнованием.
    /// </summary>
    public ICommand SaveCompetitionCommand { get; } = null!;
    
    /// <summary>
    /// Команда добавления проводящей организации.
    /// </summary>
    public ICommand AddConductingOrganizationCommand { get; } = null!;

    /// <summary>
    /// Команда удаления проводящей организации.
    /// </summary>
    public ICommand RemoveConductingOrganizationCommand { get; } = null!;

    #endregion

    #region [---------- Команды ― Судьи ----------]

    /// <summary>
    /// Команда загрузки списка судей.
    /// </summary>
    public ICommand GetRefereesCommand { get; } = null!;
    
    /// <summary>
    /// Команда добавления судьи.
    /// </summary>
    public ICommand AddRefereeCommand { get; } = null!;

    /// <summary>
    /// Команда удаления судьи.
    /// </summary>
    public ICommand RemoveRefereeCommand { get; } = null!;
    
    /// <summary>
    /// Команда сохранения изменений, связанных с судьями.
    /// </summary>
    public ICommand SaveRefereesCommand { get; } = null!;
    
    /// <summary>
    /// Команда перенумерования судей.
    /// </summary>
    public ICommand RenumberRefereesCommand { get; } = null!;

    #endregion

    
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
        RefereeService refereeService)
    {
        _view = view;
        StatusBarService = statusBarService;
        _logger = logger;
        _appSetting = appSetting;
        _competitionDataService = competitionDataService;
        _refereeService = refereeService;
        var localization = appSetting.AppLocalization;

        // Соревнования
        AddConductingOrganizationCommand = new AsyncRelayCommand(OnAddConductingOrganization);
        RemoveConductingOrganizationCommand = new AsyncRelayCommand(OnRemoveConductingOrganization);
        GetCompetitionCommand = new AsyncRelayCommand<int>(
            OnGetCompetitionDataAsync,
            id => id > 0 // CanExecute: только если Id корректный);
        );
        SaveCompetitionCommand = new AsyncRelayCommand(OnSaveCompetitionDataAsync);
        
        // Судьи
        GetRefereesCommand = new AsyncRelayCommand(OnGetRefereesAsync);
        AddRefereeCommand = new AsyncRelayCommand(OnAddRefereeAsync);
        RemoveRefereeCommand = new AsyncRelayCommand(OnRemoveReferee);
        SaveRefereesCommand = new AsyncRelayCommand(OnSaveReferees);
        
        // Привязываем команду перенумерования к методу в сервисе для работы с судьями
        RenumberRefereesCommand =
            new RelayCommand<ObservableCollection<Referee>>(_ => _refereeService.RenumberRefereesCollection(Referees));
        
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

        // Проверка, прислали ли исключение "сверху"?
        if (exceptionsProvider.Exception is not null)
        {
            // Исключение прислали - отображаем его в статус-баре и пишем в лог
            _ = StatusBarData.SetTextAsync(exceptionsProvider.Exception.Message, 
                StatusBarData.StatusBarTextType.Error, 0);
            _logger.Error(exceptionsProvider.Exception, "{class}.{method}",
                typeof(SettingVM), "CTOR");
        }
        
        if (exceptionsProvider.Exception is null || exceptionsProvider is {Exception: not null, IsFatal: false})
        {
            // Исключения нет (или оно есть, но не фатально) - получаем асинхронно данные с обработкой ошибки
            _ = InitAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    var exception = new AppException(AppPhrases.UnknownError, t.Exception);
                    _ = StatusBarData.SetTextAsync(exception.Message, 
                        StatusBarData.StatusBarTextType.Error, 0);
                    _logger.Error(exception, "{class}.{method}",
                        typeof(SettingVM), "CTOR");
                }
            });
        }
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    public void Receive(LocalizationMessage message)
    {
        var localization = _appSetting.AppLocalization;
        
        var lang = message.Lang;            // устанавливаемый язык
        var oldLang = message.OldLang;      // предыдущий язык
        
        // Перевод наименований всех доступных языков приложения в соответствии с устанавливаемым языком
        localization.Translate(localization.SetCurrentLang(lang).GetCultureInfo());

        _ = Task.Run(() =>
        {
            if (! _localizationHelper.LocalizeView(_view, lang))
            {
                // Если локализовать не получилось - переводим доступные языки обратно,
                // в соответствии с предыдущим языком
                localization.Translate(localization.SetCurrentLang(oldLang).GetCultureInfo());
            }
        });
        var currLang = localization.GetCurrentOrDefaultLang();

        // Меняем текущую локализацию и локализацию фраз приложения в соответствии с текущим языком
        CultureInfo.CurrentUICulture = currLang.GetCultureInfo();
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture;
        AppPhrases.Culture = CultureInfo.CurrentUICulture;
        
        // Изменяем свойства, используемые для биндинга
        SetProperty(ref _currLang!, null);      // отвязываем привязку свойства к событию изменения значения 
        Languages.Clear();
        localization.Languages.ForEach(item => Languages.Add(item.Value));
        SetProperty(ref _currLang, currLang, nameof(CurrLang));     // вновь привязываем
    }

    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="AllCompetitionsMessage"/>.
    /// </summary>
    public void Receive(AllCompetitionsMessage message)
    {
        CompetitionDataCollection = message.CompetitionDataCollection;
        CurrentCompetitionData = message.CurrentCompetitionData;
    }
        
    /// <summary>
    /// Инициализация - получение всех необходимых данных.
    /// </summary>
    private async Task InitAsync()
    {
        // TODO: Возможно нужно сделать проверку результатов вызовов, и если false - делать выход
        await OnGetDetailedCompetitionStatusesAsync();
        await OnGetRefereeLevels();
        await OnGetRefereeJobTitles();
        await OnGetRefereesAsync();
    }

    #region [---------- Функции для команд ― Данные о соревн. ----------]

    /// <summary>
    /// Получение статусов и наименований соревнований.
    /// </summary>
    private async Task OnGetDetailedCompetitionStatusesAsync()
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
                typeof(SettingVM), nameof(OnGetDetailedCompetitionStatusesAsync));
        }
    }

    /// <summary>
    /// Добавление проводящей организации.
    /// </summary>
    private Task OnAddConductingOrganization()
    {
        // Создаем новую организацию
        var newIndexResult = _competitionDataService.AddConductingOrganization(ConductingOrganizations, 
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
                typeof(SettingVM), nameof(OnAddConductingOrganization));
        }

        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Удаление проводящей организации.
    /// </summary>
    private Task OnRemoveConductingOrganization()
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
                typeof(SettingVM), nameof(OnRemoveConductingOrganization));
        }

        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Получение полных данных о соревновании.
    /// </summary>
    private async Task OnGetCompetitionDataAsync(int id)
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
            var existing = CompetitionDataCollection.FirstOrDefault(c => c.Id == id);
            if (existing != null)
            {
                var index = CompetitionDataCollection.IndexOf(existing);
                CompetitionDataCollection[index] = competitionDataResult.Value!;    // замена
            }
            else
            {
                CompetitionDataCollection.Add(competitionDataResult.Value!);        // добавление
            }
            
            // Заменяем текущие соревнования (меняем через поле, чтобы не вызвать данный метод повторно)
            SetProperty(ref _currentCompetitionData, competitionDataResult.Value, nameof(CurrentCompetitionData));
            
            // Посылаем сообщение об изменении текущего соревнования
            Messenger.Send(new CompetitionMessage(CurrentCompetitionData));
            
            // Заполняем проводящие организации
            var intResult = _competitionDataService.GetConductingOrganizations(
                CurrentCompetitionData, ConductingOrganizations);
            if (! intResult)
            {
                exception = intResult.Excptn;
            }
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
                    typeof(SettingVM), nameof(OnGetCompetitionDataAsync));
            }
        }
    }

    /// <summary>
    /// Сохранение соревнования.
    /// </summary>
    private async Task OnSaveCompetitionDataAsync()
    {
        // Сохраняем проводящие организации
        _competitionDataService.SetConductingOrganizations(CurrentCompetitionData, ConductingOrganizations);
        
        // Сохраняем изменения
        var intResult = await _competitionDataService.SaveCompetitionDataAsync();
        if (intResult)
        {
            // Уведомляем UI об изменении
            OnPropertyChanged(nameof(CurrentCompetitionData));
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(intResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(intResult.Excptn,
                "{class}.{method}.",
                typeof(SettingVM), nameof(OnSaveCompetitionDataAsync));
        }
    }
    
    #endregion
    
    #region [---------- Функции для команд ― Судьи ----------]

    /// <summary>
    /// Получение судейских категорий.
    /// </summary>
    private async Task OnGetRefereeLevels()
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
                typeof(SettingVM), nameof(OnGetRefereeLevels));
        }
    }
    
    /// <summary>
    /// Получение судейских должностей.
    /// </summary>
    private async Task OnGetRefereeJobTitles()
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
                typeof(SettingVM), nameof(OnGetRefereeJobTitles));
        }
    }
    
    /// <summary>
    /// Получение списка судей.
    /// </summary>
    private async Task OnGetRefereesAsync()
    {
        var refereesResult = await _refereeService.GetRefereesAsync(Referees);
        if (! refereesResult)
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(refereesResult.Excptn?.Message,
                BaseException.ExcptnType.Error, 0);
            _logger.Error(refereesResult.Excptn, "{class}.{method}",
                typeof(SettingVM), nameof(OnGetRefereesAsync));
        }
    }

    /// <summary>
    /// Добавление судьи.
    /// </summary>
    private async Task OnAddRefereeAsync()
    {
        var refereesResult = await _refereeService.AddRefereeAsync(Referees, Referees.SelectedIndex);
        if (refereesResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereesResult.Value;
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarService.SetTextAsync("Добавили судью.", BaseException.ExcptnType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(refereesResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);
            _logger.Error(refereesResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnAddRefereeAsync));
        }
    }

    /// <summary>
    /// Удаление судьи.
    /// </summary>
    private Task OnRemoveReferee()
    {
        var refereesResult = _refereeService.RemoveReferee(Referees, Referees.SelectedIndex);
        if (refereesResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereesResult.Value;
            
            // TODO: Временно (без ожидания окончания)
            if (refereesResult.Value >= 0)
                _ = StatusBarService.SetTextAsync("Удалили судью.", BaseException.ExcptnType.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(refereesResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);            
            _logger.Error(refereesResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnRemoveReferee));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Сохранение судей.
    /// </summary>
    private async Task OnSaveReferees()
    {
        // Сохраняем изменения
        var intResult = await _refereeService.SaveRefereesAsync();
        if (! intResult)
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarService.SetTextAsync(intResult.Excptn?.Message, 
                BaseException.ExcptnType.Error, 0);            
            _logger.Error(intResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnSaveReferees));
        }
        else
        {
            // Уведомляем UI об изменении
            OnPropertyChanged(nameof(Referees));
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
        if (disposing)
        {
            _appSetting.SaveConfig();
        }
    }
    
    /// <inheritdoc />
    ~SettingVM() => Dispose(false);
}
