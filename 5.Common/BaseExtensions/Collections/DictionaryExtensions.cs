using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.BaseExtensions.Collections;

/// <summary>
/// Методы-расширения для <see cref="IDictionary{TKey, TValu}" />.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Добавление в словарь с предварительным удалением ключа, если он уже есть.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="dictionary">Словарь.</param>
    /// <param name="key">Ключ.</param>
    /// <param name="value">Добавляемое значение.</param>
    public static void AddWithRemove<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (dictionary.ContainsKey(key))
            dictionary.Remove(key);
        dictionary.Add(key, value);
    }

    /// <summary>
    /// Добавление только уникальных элементов.
    /// </summary>
    /// <typeparam name="TKey">Тип ключа.</typeparam>
    /// <typeparam name="TValue">Тип значения.</typeparam>
    /// <param name="dictionary">Словарь.</param>
    /// <param name="key">Ключ.</param>
    /// <param name="value">Добавляемое значение.</param>
    public static void AddUnique<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
            
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
    }

    /// <summary>
    /// Получение ключа словаря (Dictionary) по значению.
    /// </summary>
    /// <remarks>
    /// Использовать с осторожностью! Словарь не имеет уникальности значений.
    /// </remarks>
    /// <param name="this">Словарь, с которым происходит работа.</param>
    /// <param name="value">Значение словаря, к которому необходимо получить ключ.</param>
    /// <param name="key">Получаемый ключ.</param>
    /// <returns>True - если ключ найден, false - в противном случае.</returns>
    public static bool KeyByValue<TKey, TValue>
        (this Dictionary<TKey, TValue> @this, TValue value, out TKey key) where TValue : IEquatable<TValue>
    {
        if (@this.ContainsValue(value))
        {
            key = @this.FirstOrDefault(x =>  x.Value.Equals(value)).Key;
            return true;
        }

        key = default;
        return false;
    }
}