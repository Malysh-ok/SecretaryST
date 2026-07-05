using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Common.WpfModule.Components.Collections;

/// <summary>
/// Расширенная Observable-коллекция.
/// </summary>
public class ObservableCollectionEx<T>: ObservableCollection<T>
{
    private Action<int>? _onSelectedIndexChanged;
    private Action? _onCollectionChanged;
    
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
            _onSelectedIndexChanged?.Invoke(value);
        }
    } = -1;

    /// <summary>
    /// Добавление коллекции элементов.
    /// </summary>
    public void AddRange(IList<T>? enumerable)
    {
        if (enumerable is null)
            return;
        
        // Проверяем на возможность изменения
        CheckReentrancy();

        if (enumerable.Count > 0)
        {
            foreach (var item in enumerable)
                Items.Add(item);

            // Вызываем необходимые события
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
        }
    }

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
    
    /// <summary>
    /// Устанавливает функцию обратного вызова на изменение SelectedIndex,
    /// где параметр этой функции - новое значение SelectedIndex.
    /// </summary>
    public void OnSelectedIndexChanged(Action<int> callback)
    {
        _onSelectedIndexChanged = callback;
    }

    /// <summary>
    /// Устанавливает функцию обратного вызова на любое изменение коллекции (Add, Remove, Clear, Replace)
    /// </summary>
    public void OnCollectionChanged(Action callback)
    {
        _onCollectionChanged = callback;
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        _onCollectionChanged?.Invoke();

        // Если коллекция изменилась, и SelectedIndex выходит за пределы — сбрасываем
        if (SelectedIndex >= Count)
            SelectedIndex = Count - 1;
    }
}