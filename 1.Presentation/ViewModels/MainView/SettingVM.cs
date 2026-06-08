using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using AppDomain.Phrases;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;
using AppDomain.UseCases.Services;
using Common.BaseExtensions.Collections;
using Common.WpfModule.Components.Collections;
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class SettingVM : ObservableRecipient, IRecipient<LocalizationMessage>, IStatusBarDataProvider, IDisposable
{
    private readonly LocalizationHelper _localizationHelper = null!;
    private readonly IViewWithResources _view = null!;
    private readonly ILogger _logger = null!;
    private readonly AppSettingService _appSetting = null!;
    private readonly CompetitionDataService _competitionDataService = null!;
    private readonly RefereeService _refereeService = null!;

    /// <summary>
    /// Список доступных языков.
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
    /// Данные для статус-бара.
    /// </summary>
    public StatusBarData StatusBarData { get; } = null!;

    // TODO: Разобраться, зачем нам понадобилось свойство (DataGridCellInfo)CellInfo.
    public DataGridCellInfo CellInfo
    {
        get;
        set
        {
            if (value.IsValid)
                SetProperty(ref field, value);
        }
    }

    #region [---------- Данные о соревнованиях ----------]

    /// <summary>
    /// Список статусов соревнований.
    /// </summary>
    public ObservableCollectionEx<CompetitionsStatus> CompetitionsStatuses { get; set; } = [];
    
    /// <summary>
    /// Список статусов и обобщенных наименований соревнований.
    /// </summary>
    public ObservableCollectionEx<DetailedCompetitionStatus> DetailedCompetitionStatuses { get; set; } = [];

    /// <summary>
    /// Данные о соревнования.
    /// </summary>
    public CompetitionData? CompetitionData
    {
        get;
        private set => SetProperty(ref field, value);
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
    /// Команда загрузки данных о соревнованиях.
    /// </summary>
    public ICommand LoadCompetitionCommand { get; } = null!;
    
    /// <summary>
    /// Команда сохранения изменений, связанных с соревнованиями.
    /// </summary>
    public ICommand SaveCompetitionDataCommand { get; } = null!;

    #endregion
    

    #region [---------- Команды ― Судьи ----------]

    /// <summary>
    /// Команда загрузки списка судей.
    /// </summary>
    public ICommand LoadRefereesCommand { get; } = null!;
    
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

    #endregion

    
    /// <summary>
    /// Конструктор, запрещающий создания экземпляра без параметров.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private SettingVM()
    {
    }
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public SettingVM(IViewWithResources view, 
        StatusBarData statusBarData, 
        ILogger logger, 
        IExceptionsProvider exceptionsProvider,
        AppSettingService appSetting,
        CompetitionDataService competitionDataService,
        RefereeService refereeService)
    {
        _view = view;
        StatusBarData = statusBarData;
        _logger = logger;
        _appSetting = appSetting;
        _competitionDataService = competitionDataService;
        _refereeService = refereeService;
        var localization = appSetting.AppLocalization;
        
        LoadCompetitionCommand = new AsyncRelayCommand(OnGetCompetitionData);
        SaveCompetitionDataCommand = new AsyncRelayCommand(OnSaveCompetitionData);
        
        LoadRefereesCommand = new AsyncRelayCommand(OnGetReferees);
        AddRefereeCommand = new AsyncRelayCommand(OnAddReferee);
        RemoveRefereeCommand = new AsyncRelayCommand(OnRemoveReferee);
        SaveRefereesCommand = new AsyncRelayCommand(OnSaveReferees);
        
        // Установка локализации из настроек
        Languages = new ObservableCollection<Lang>(localization.Languages.Values);
        var langName = localization.GetLangFromSetting();
        _currLang = localization.SetCurrentLangFromName(langName);
        _localizationHelper = new LocalizationHelper(appSetting);

        // Подписываемся на получение сообщений
        Messenger.Register(this);
        
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
            _logger.Error(exceptionsProvider.Exception, "");
        }
        
        if (exceptionsProvider.Exception is null || exceptionsProvider is {Exception: not null, IsFatal: false})
        {
            // Исключения нет (или оно есть, не не фатально) - получаем синхронно данные
            Init().GetAwaiter().GetResult();
        }
    }
    
    /// <summary>
    /// Получаем сообщение с экземпляром <see cref="LocalizationMessage"/>.
    /// </summary>
    public async void Receive(LocalizationMessage message)
    {
        var localization = _appSetting.AppLocalization;
        
        var lang = message.Lang;            // устанавливаемый язык
        var oldLang = message.OldLang;      // предыдущий язык
        
        // Перевод всех доступных языков приложения в соответствии с устанавливаемым языком
        localization.Translate(localization.SetCurrentLang(lang).GetCultureInfo());

        if (!await _localizationHelper.LocalizeView(_view, lang))
        {
            // Если локализовать не получилось - переводим доступные языки обратно,
            // в соответствии с предыдущим языком
            localization.Translate(localization.SetCurrentLang(oldLang).GetCultureInfo());
        }
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
    /// Инициализация - получение всех необходимых данных.
    /// </summary>
    private async Task Init()
    {
        await OnGetDetailedCompetitionStatuses();
        await OnGetCompetitionData();
        await OnGetRefereeLevels();
        await OnGetRefereeJobTitles();
        await OnGetReferees();
    }

    #region [---------- Функции для команд ― Данные о соревн. ----------]

    /// <summary>
    /// Получение статусов и наименований соревнований.
    /// </summary>
    private async Task OnGetDetailedCompetitionStatuses()
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
            _ = StatusBarData.SetTextAsync(detailedCompetitionsStatusesResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(detailedCompetitionsStatusesResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(OnGetDetailedCompetitionStatuses));
        }
    }

    /// <summary>
    /// Получение данных о соревнованиях.
    /// </summary>
    private async Task OnGetCompetitionData()
    {
        var competitionDataResult = await _competitionDataService.GetCompetitionDataAsync();
        if (competitionDataResult)
        {
            // Обновляем свойство с данными о соревнованиях
            CompetitionData = null;
            CompetitionData = competitionDataResult.Value;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(competitionDataResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(competitionDataResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(OnGetCompetitionData));
        }
    }

    /// <summary>
    /// Сохранение данных о соревнованиях.
    /// </summary>
    private async Task OnSaveCompetitionData()
    {
        var intResult = await _competitionDataService.SaveCompetitionDataAsync(CompetitionData!);

        if (! intResult)
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(intResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(intResult.Excptn, 
                "{class}.{method}.", 
                typeof(SettingVM), nameof(OnSaveCompetitionData));
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
            _ = StatusBarData.SetTextAsync(refereeLevelsResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
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
            _ = StatusBarData.SetTextAsync(refereeJobTitlesResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(refereeJobTitlesResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnGetRefereeJobTitles));
        }
    }
    
    /// <summary>
    /// Получение списка судей.
    /// </summary>
    private async Task OnGetReferees()
    {
        var refereesResult = await _refereeService.GetRefereesAsync(Referees, Referees.SelectedIndex);
        if (refereesResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereesResult.Value;
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(refereesResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(refereesResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnGetReferees));
        }
    }

    /// <summary>
    /// Добавление судьи.
    /// </summary>
    private async Task OnAddReferee()
    {
        var refereesResult = await _refereeService.AddRefereeAsync(Referees, Referees.SelectedIndex);
        if (refereesResult)
        {
            // Перезаписываем индекс
            Referees.SelectedIndex = refereesResult.Value;
            
            // TODO: Временно (без ожидания окончания)
            _ = StatusBarData.SetTextAsync("Добавили судью.", StatusBarData.StatusBarTextType.Info);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(refereesResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);
            _logger.Error(refereesResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnAddReferee));
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
                _ = StatusBarData.SetTextAsync("Удалили судью.", StatusBarData.StatusBarTextType.Error);
        }
        else
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(refereesResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
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
        var intResult = await _refereeService.SaveRefereesAsync(Referees);

        if (! intResult)
        {
            // Пишем в статус-бар и лог об ошибке
            _ = StatusBarData.SetTextAsync(intResult.Excptn?.Message, 
                StatusBarData.StatusBarTextType.Error, 0);            
            _logger.Error(intResult.Excptn, "{class}.{method}", 
                typeof(SettingVM), nameof(OnSaveReferees));
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
