using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AppDomain.Phrases;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
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

    #endregion
    

    #region [---------- Команды ― Данные о соревн. ----------]
        
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

    #region [---------- Команды ― Судьи ----------]

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
        CreateConductingOrganizationCommand = new AsyncRelayCommand(CreateConductingOrganization);
        RemoveConductingOrganizationCommand = new AsyncRelayCommand(RemoveConductingOrganization);
        GetCompetitionCommand = new AsyncRelayCommand<int>(
            GetCompetitionDataAsync,
            id => id > 0 // CanExecute: только если Id корректный);
        );
        SaveCompetitionCommand = new AsyncRelayCommand(SaveCompetitionAsync);
        
        // Судьи
        GetRefereesCommand = new AsyncRelayCommand(GetRefereesAsync);
        CreateRefereeCommand = new AsyncRelayCommand(CreateRefereeAsync);
        RemoveRefereeCommand = new AsyncRelayCommand(RemoveReferee);
        
        
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
    }

    #region [---------- Функции для команд ― Данные о соревн. ----------]

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
    
    #region [---------- Функции для команд ― Судьи ----------]

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
        var refereesResult = await _refereeService.CreateRefereeAsync(
            Referees, Referees.SelectedIndex, CurrentCompetition);
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
                typeof(SettingVM), nameof(CreateRefereeAsync));
        }
    }

    /// <summary>
    /// Удаление судьи.
    /// </summary>
    private Task RemoveReferee()
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
                typeof(SettingVM), nameof(RemoveReferee));
        }

        return Task.CompletedTask;
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
