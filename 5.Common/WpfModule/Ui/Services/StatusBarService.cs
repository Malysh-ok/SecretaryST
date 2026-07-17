using System.Drawing;
using System.Windows.Media;
using Common.BaseComponents.Components.Exceptions;
using Common.WpfModule.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Brush = System.Windows.Media.Brush;

namespace Common.WpfModule.Ui.Services;

/// <summary>
/// Сервис для работы со статус-баром.
/// </summary>
public class StatusBarService : ObservableObject
{
    /// <summary>
    /// Токен для отмены времени показа текста статус-бара.
    /// </summary>
    private static CancellationTokenSource _ctsVisualizationTime = new();

    /// <summary>
    /// Время показа информации в мсек. в статус-баре по умолчанию.
    /// </summary>
    public static int VisualizationTimeDefault { get; protected set; } = 4000;

    /// <summary>
    /// Время показа информации в мсек. в статус-баре.
    /// </summary>
    public static int VisualizationTime { get; protected set; }

    /// <summary>
    /// Кисть для информации.
    /// </summary>
    public static Brush InfoBrush { get; protected set; } = 
        new SolidColorBrush(SystemColors.ControlText.ToMediaColor());

    /// <summary>
    /// Кисть для предупреждения.
    /// </summary>
    public static Brush WarningBrush { get; protected set; } = 
        new SolidColorBrush(SystemColors.ControlText.ToMediaColor());

    /// <summary>
    /// Кисть для ошибки.
    /// </summary>
    public static Brush ErrorBrush { get; protected set; } = 
        new SolidColorBrush(Colors.Firebrick);

    /// <summary>
    /// Текст статус-бара.
    /// </summary>
    public string StatusText
    {
        get;
        protected set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Кисть текста статус-бара.
    /// </summary>
    public Brush Brush
    {
        get;
        protected set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Прогресс-бар внутри статус-бара.
    /// </summary>
    public int ProgressValue
    {
        get;
        protected set
        {
            if (SetProperty(ref field, value))
            {
                // При изменении ProgressValue автоматически обновляем IsProgressVisible
                OnPropertyChanged(nameof(IsProgressVisible));
            }
        }
    }

    /// <summary>
    /// Признак видимости прогресс-бара.
    /// </summary>
    public bool IsProgressVisible => ProgressValue > 0;

    #region [---------- Скрытые методы ----------]

    /// <summary>
    /// Установка кисти по умолчанию.
    /// </summary>
    private void SetBrushDefault()
    {
        Brush = InfoBrush;
    }
    
    #endregion
    
    /// <summary>
    /// Конструктор.
    /// </summary>
    public StatusBarService()
    {
        StatusText = string.Empty;
        VisualizationTime = VisualizationTimeDefault;
        Brush = null!;
        SetBrushDefault();
    }
    
    /// <summary>
    /// Переустановка кистей статус-бара.
    /// </summary>
    /// <param name="infoBrush">Кисть для информации.</param>
    /// <param name="warningBrush">Кисть для предупреждения.</param>
    /// <param name="errorBrush">Кисть для ошибки.</param>
    public void ReSetBrushes(Brush? infoBrush = null, Brush? warningBrush = null, Brush? errorBrush = null)
    {
        InfoBrush = infoBrush ?? InfoBrush;
        WarningBrush = warningBrush ?? WarningBrush;
        ErrorBrush = errorBrush ?? ErrorBrush;
        
        SetBrushDefault();
    }

    /// <summary>
    /// Устанавливаем данные в статус-баре.
    /// </summary>
    /// <param name="statusText">Текст для отображения.</param>
    /// <param name="brush">Кисть для текста.</param>
    /// <param name="visualizationTime">Время отображения, после которого текст стирается.</param>
    /// <param name="isHideProgressBar">Признак скрытия прогресс-бара.</param>
    /// <remarks>
    /// Если <paramref name="visualizationTime"/> меньше или равен 0 - текст стираться не будет.
    /// </remarks>
    public async Task SetTextAsync(string? statusText = null, 
        Brush? brush = null, 
        int? visualizationTime = null,
        bool isHideProgressBar = true)
    {
        // Отменяем предыдущее стирание текста в статус баре
        await _ctsVisualizationTime.CancelAsync();
        _ctsVisualizationTime.Dispose();
        _ctsVisualizationTime = new CancellationTokenSource();
        
        StatusText = statusText ?? string.Empty;
        Brush = brush ?? Brush;
        VisualizationTime = visualizationTime ?? VisualizationTimeDefault;
        if (isHideProgressBar)
        {
            // Скрываем прогресс-бар
            ProgressValue = 0;
        }

        try
        {
            if (VisualizationTime > 0)
            {
                // Если заданное время визуализации больше 0, то по его окончании - стираем текст
                await Task.Delay(VisualizationTime, _ctsVisualizationTime.Token);
                StatusText = string.Empty;
            }
        }
        catch (OperationCanceledException)
        {
            // Операция была отменена новым вызовом - ничего не делаем,
            // сообщение уже обновлено этим вызовом
        }
    }

    /// <summary>
    /// Устанавливаем данные в статус-баре.
    /// </summary>
    /// <param name="statusText">Текст для отображения.</param>
    /// <param name="textType">Тип текста: информация, предупреждение, ошибка.</param>
    /// <param name="visualizationTime">Время отображения, после которого текст стирается.</param>
    /// <param name="isHideProgressBar">Признак скрытия прогресс-бара.</param>
    /// <remarks>
    /// Если <paramref name="visualizationTime"/> меньше или равен 0 - текст стираться не будет.
    /// </remarks>
    public async Task SetTextAsync(string? statusText = null,
        ExcptnTypeEnm textType = ExcptnTypeEnm.Info, 
        int? visualizationTime = null,
        bool isHideProgressBar = true)
    {
        var brush = textType switch
        {
            ExcptnTypeEnm.Info => InfoBrush,
            ExcptnTypeEnm.Warning => WarningBrush,
            ExcptnTypeEnm.Error => ErrorBrush,
            _ => throw new ArgumentOutOfRangeException(nameof(textType), textType, null)
        };
        await SetTextAsync(statusText, brush, visualizationTime, isHideProgressBar);
    }

    /// <summary>
    /// Устанавливаем значение прогресс-бара в статус-баре.
    /// </summary>
    /// <param name="progressValue">Значение прогресс-бара.</param>
    public Task SetProgressAsync(int? progressValue = null)
    {
        ProgressValue = progressValue ?? 0;
        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public override string ToString()
        => StatusText;
}
