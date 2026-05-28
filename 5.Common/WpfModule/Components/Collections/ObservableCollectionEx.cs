using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Common.WpfModule.Components.Collections;

/// <summary>
/// Расширенная Observable-коллекция.
/// </summary>
public class ObservableCollectionEx<T>: ObservableCollection<T>
{
    private int _selectedIndex = -1;
    /// <summary>
    /// Индекс выделенного элемента коллекции.
    /// </summary>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (_selectedIndex == value)
                return;
            
            // Проверяем на возможность изменения
            CheckReentrancy();

            _selectedIndex = -1;
            if (value >= 0 && value < Items.Count)
                _selectedIndex = value;
            
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedIndex)));
        }
    }

    /// <summary>
    /// Добавление коллекции элементов.
    /// </summary>
    public void AddRange(IEnumerable<T>? enumerable)
    {
        if (enumerable is null)
            return;
        
        // Проверяем на возможность изменения
        CheckReentrancy();

        foreach (var item in enumerable)
            Items.Add(item);

        // Вызываем необходимые события
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
    }
    
        #region [---------- Сортировка ----------]
        
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

        #endregion // [---------- Сортировка ----------]
}