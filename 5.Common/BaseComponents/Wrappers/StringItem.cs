using CommunityToolkit.Mvvm.ComponentModel;

namespace Common.BaseComponents.Wrappers;

/// <summary>
/// Обёртка для строкового значения с поддержкой уведомлений об изменениях.
/// </summary>
public class StringItem : ObservableObject
{
    private string _value = string.Empty;

    /// <summary>
    /// Строковое значение элемента.
    /// При изменении автоматически вызывает уведомление для обновления UI.
    /// </summary>
    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    /// <summary>
    /// Конструктор.
    /// </summary>
    public StringItem()
    {
    }

    /// <summary>
    /// Конструктор с параметром.
    /// </summary>
    /// <param name="value">Начальное строковое значение</param>
    public StringItem(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Возвращает строковое представление элемента.
    /// </summary>
    public override string ToString() => _value;
}