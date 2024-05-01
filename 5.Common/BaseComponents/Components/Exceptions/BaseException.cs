#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Common.BaseExtensions;

namespace Common.BaseComponents.Components.Exceptions;

/// <summary>
/// Класс базового исключения.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class BaseException : Exception
{
    /// <summary>
    /// Создание объекта с помощью конструктора запрещено.
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    private BaseException(string? message = null, Exception? innerException = null) 
        : base(message, innerException)
    {
    }
        
    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="message">Сообщение об ошибке в текущей локализации,
    /// указывающее причину создания исключения.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="localLangName">Наименование альтернативной локализации.</param>
    /// <param name="localMessage">Сообщение об ошибке в альтернативной локализации,
    /// указывающее причину создания исключения.</param>
    public BaseException(string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        : base(GetRealMessage(message, localLangName, localMessage), innerException)
    {
    }

    
    /// <summary>
    /// Создает новый экземпляр исключения <see cref="BaseException" /> (фабричный метод).
    /// </summary>
    /// <remarks>
    /// Подставляется либо сообщение об ошибке <see cref="localMessage" />
    /// (если наименование текущей локализации равно <see cref="localLangName" />),
    /// либо <see cref="message" /> (в противном случае).
    /// </remarks>
    /// <param name="message">Сообщение об ошибке в текущей локализации,
    /// указывающее причину создания исключения.</param>
    /// <param name="innerException">Исключение, вызвавшее текущее исключение, или null.</param>
    /// <param name="localLangName">Наименование альтернативной локализации.</param>
    /// <param name="localMessage">Сообщение об ошибке в альтернативной локализации,
    /// указывающее причину создания исключения.</param>
    public static BaseException CreateException(
        string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null)
        => new(message, innerException, localLangName, localMessage);

    
    /// <inheritdoc cref="CreateException(string, Exception, string, string)"/>
    /// <summary>
    /// Создает новый экземпляр исключения <see cref="TEx"/> (фабричный метод).
    /// Тип <see cref="TEx"/> должен быть в цепи предков-потомков класса <see cref="BaseException"/>.
    /// </summary>
    public static TEx CreateException<TEx>(
        string? message = null, Exception? innerException = null,
        string? localLangName = null, string? localMessage = null) where TEx: Exception
    {
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            
        var constructor = typeof(TEx).GetConstructor(bindingFlags,
            null, [typeof(string), typeof(Exception), typeof(string), typeof(string)], 
            null);
        if (constructor != null)
            return (TEx)constructor.Invoke([message, innerException, localLangName, localMessage]);
        
        // Получаем сообщение, в зависимости от языка.
        if (! localLangName.IsNullOrEmpty())
            message = GetRealMessage(message, localLangName, localMessage);
        
        constructor = typeof(TEx).GetConstructor(bindingFlags,
            null, [typeof(string), typeof(Exception)], 
            null);
        if (constructor != null)
            return (TEx)constructor.Invoke([message, innerException]);
                
        constructor = typeof(TEx).GetConstructor(bindingFlags,
            null, [typeof(string)], 
            null);
        if (constructor != null)
            return (TEx)constructor.Invoke([message]);

        constructor = typeof(TEx).GetConstructor(bindingFlags,
            null, [], 
            null);
        if (constructor != null)
            return (TEx)constructor.Invoke(null);

        // Ошибка
        throw CreateException($"Unable to instantiate {typeof(TEx)}.", null,
            "ru", $"Невозможно создать экземпляр {typeof(TEx)}.");
    }

    
    #region [---------- НЕ публичные члены ----------]

    /// <summary>
    /// Получить реальное сообщение, в зависимости от языкового стандарта.
    /// </summary>
    protected static string? GetRealMessage(string? message, string? localLangName, string? localMessage,
        CultureInfo? culture = null)
    {
        var currCulture = culture ?? CultureInfo.CurrentUICulture;
            
        return currCulture.TwoLetterISOLanguageName == localLangName 
               || currCulture.Name == localLangName
            ? localMessage
            : message;
    }

    /// <summary>
    /// Преобразование сообщения в нужную локализацию из ресурсов.
    /// </summary>
    /// <param name="resourceClassType">Тип класса ресурсов.</param>
    /// <param name="message">Сообщение.</param>
    /// <param name="localLangName">Название локализации, к которой преобразукем сообщение.</param>
    protected static string? ConvertMessage(Type resourceClassType, string? message, string? localLangName)
    {
        if (localLangName is null)
            return message;

        string? messagePropName = null;
        string? cultureInfoPropName = null;
        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;

            
        var propertyInfos = resourceClassType.GetProperties(bindingFlags);
        foreach (var prop in propertyInfos)
        {
            if (prop.PropertyType == typeof(string) && prop.GetValue(prop.Name) as string == message)
            {
                messagePropName = prop.Name;
            }
                
            if (prop.PropertyType == typeof(CultureInfo))
            {
                cultureInfoPropName = prop.Name;
            }
        }

        if (messagePropName is null || cultureInfoPropName is null)
            return message;
            
        resourceClassType.GetProperty(cultureInfoPropName)?
            .SetValue(cultureInfoPropName, CultureInfo.GetCultureInfo(localLangName));
        var result = resourceClassType.GetProperty(messagePropName)?.GetValue(messagePropName) as string;

        return result;
    }

    #endregion
}