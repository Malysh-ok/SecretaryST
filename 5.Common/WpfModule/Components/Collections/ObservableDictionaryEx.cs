using System.ComponentModel;
using Cogs.Collections;

namespace Common.WpfModule.Components.Collections;

public class ObservableDictionaryEx<TKey, TValue> : ObservableDictionary<TKey, TValue>
{
    private Action<int>? _onSelectedIndexChanged;
    private Action? _onDictionaryChanged;

    /// <inheritdoc/>
    public ObservableDictionaryEx()
    {
    }

    /// <inheritdoc/>
    public ObservableDictionaryEx(IDictionary<TKey, TValue> dictionary)
        : base(dictionary)
    {
    }

    /// <inheritdoc/>
    public ObservableDictionaryEx(IEqualityComparer<TKey> comparer)
        : base(comparer)
    {
    }
    
    /// <inheritdoc/>
    public ObservableDictionaryEx(int capacity)
        : base(capacity)
    {
    }
    
    /// <inheritdoc/>
    public ObservableDictionaryEx(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        : base(dictionary, comparer)
    {
    }

    /// <inheritdoc/>
    public ObservableDictionaryEx(int capacity, IEqualityComparer<TKey> comparer)
        : base(capacity, comparer)
    {
    }
    
    /// <summary>
    /// Индекс активного элемента словаря.
    /// </summary>
    /// <remarks>
    /// Нумерация начинается с 0.
    /// </remarks>
    public int SelectedIndex
    {
        get;
        set
        {
            if (field == value)
                return;

            field = -1;
            if (value >= 0 && value < Count)
                field = value;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedIndex)));
            _onSelectedIndexChanged?.Invoke(value);
        }
    } = -1;

    /// <summary>
    /// Устанавливает функцию обратного вызова на изменение SelectedIndex,
    /// где параметр этой функции - новое значение SelectedIndex.
    /// </summary>
    public void OnSelectedIndexChanged(Action<int> callback)
    {
        _onSelectedIndexChanged = callback;
    }

    /// <summary>
    /// Устанавливает функцию обратного вызова на любое изменение словаря.
    /// </summary>
    public void OnDictionaryChanged(Action callback)
    {
        _onDictionaryChanged = callback;
    }

    protected override void OnDictionaryChanged(NotifyDictionaryChangedEventArgs<TKey, TValue> e)
    {
        base.OnDictionaryChanged(e);
        _onDictionaryChanged?.Invoke();

        // Если словарь изменился, и SelectedIndex выходит за пределы — сбрасываем
        if (SelectedIndex >= Count)
            SelectedIndex = Count - 1;
    }
}