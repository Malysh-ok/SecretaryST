using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Common.WpfModule.Components.Collections;

/// <summary>
/// Расширенная Observable-коллекция.
/// </summary>
public class ObservableCollectionEx<T>: ObservableCollection<T>
{
    public ObservableCollectionEx()
    {
    }

    public ObservableCollectionEx(IEnumerable<T> collection) 
        : base([..collection ?? throw new ArgumentNullException(nameof(collection))])
    {
    }
    
    public ObservableCollectionEx(List<T> list) 
        : base([..list ?? throw new ArgumentNullException(nameof(list))])
    {
    }
    
    /// <summary>
    /// Индекс активного элемента коллекции.
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

            // Проверяем на возможность изменения
            CheckReentrancy();

            field = -1;
            if (value >= 0 && value < Count)
                field = value;

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedIndex)));
            SelectedIndexChanged?.Invoke(field);
        }
    } = -1;
    
    /// <summary>
    /// Событие изменения индекса активного элемента коллекции.
    /// </summary>
    public event Action<int>? SelectedIndexChanged;

    /// <summary>
    /// Событие изменения коллекции.
    /// </summary>
    public new event EventHandler<NotifyCollectionChangedEventArgs>? CollectionChanged;
    
    /// <inheritdoc/>
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        CollectionChanged?.Invoke(this, e);

        // Если коллекция изменилась, и SelectedIndex выходит за пределы — берем максимальный
        if (SelectedIndex >= Count)
            SelectedIndex = Count - 1;
    }

    #region [---------- Изменения коллекции ----------]
    
    /// <summary>
    /// Обновляет коллекцию, вызывая одно событие изменения.
    /// </summary>
    private void RefreshCollection()
    {
        using (BlockReentrancy())
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
    }

    /// <summary>
    /// Очищает коллекцию и добавляет один элемент.
    /// </summary>
    public void ClearAndAdd(T item)
    {
        CheckReentrancy();
    
        Items.Clear();
        Items.Add(item);
    
        RefreshCollection();
    }
    
    /// <summary>
    /// Добавление коллекции элементов.
    /// </summary>
    public void AddRange(IEnumerable<T>? items)
    {
        if (items is null)
            return;
        
        // Проверяем, пустая ли добавляемая коллекция
        if (items is ICollection<T> { Count: 0 })
            return;
        
        // Проверяем на возможность изменения
        CheckReentrancy();
        
        foreach (var item in items)
            Items.Add(item);

        // Вызываем необходимые события
        RefreshCollection();
    }
    
    /// <summary>
    /// Очищает коллекцию и добавляет коллекцию элементов.
    /// </summary>
    public void ClearAndAddRange(IEnumerable<T>? items)
    {
        if (items is null)
            return;
            
        // Проверяем, пустая ли добавляемая коллекция
        if (items is ICollection<T> { Count: 0 })
            return;

        // Проверяем на возможность изменения
        CheckReentrancy();
    
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);
    
        RefreshCollection();
    }

    #endregion
    
    #region [---------- Сортировки ----------]

    /// <summary>
    /// Перемещает элементы коллекции так, чтобы их порядок совпадал с порядком элементов заданной коллекции.
    /// </summary>
    /// <param name="sortedItems">Заданная коллекция.</param>
    private void InternalSort(IEnumerable<T> sortedItems)
    {
        var sortedItemsList = sortedItems.ToList();

        foreach (var item in sortedItemsList)
        {
            Move(IndexOf(item), sortedItemsList.IndexOf(item));
        }
    }

    /// <summary>
    /// Сортирует элементы коллекции в порядке возрастания в соответствии с ключом.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого <paramref name="key"/>.</typeparam>
    /// <param name="key">Функция, извлекающая ключ из элемента..</param>
    public void Sort<TKey>(Func<T, TKey> key)
    {
        InternalSort(Items.OrderBy(key));
    }

    /// <summary>
    /// Сортирует элементы коллекции в порядке убывания по ключу.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого <paramref name="key"/>.</typeparam>
    /// <param name="key">Функция, извлекающая ключ из элемента..</param>
    public void SortDescending<TKey>(Func<T, TKey> key)
    {
        InternalSort(Items.OrderByDescending(key));
    }

    /// <summary>
    /// Сортирует элементы коллекции в порядке возрастания по ключу.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого <paramref name="key"/>.</typeparam>
    /// <param name="key">Функция, извлекающая ключ из элемента.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
    public void Sort<TKey>(Func<T, TKey> key, IComparer<TKey> comparer)
    {
        InternalSort(Items.OrderBy(key, comparer));
    }

    /// <summary>
    /// Сортирует элементы коллекции в порядке убывания по ключу.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа, возвращаемого <paramref name="key"/>.</typeparam>
    /// <param name="key">Функция, извлекающая ключ из элемента.</param>
    /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
    public void SortDescending<TKey>(Func<T, TKey> key, IComparer<TKey> comparer)
    {
        InternalSort(Items.OrderByDescending(key, comparer));
    }

    #endregion // [---------- Сортировки ----------]
}