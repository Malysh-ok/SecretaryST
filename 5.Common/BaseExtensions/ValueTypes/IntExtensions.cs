using System;
using System.Globalization;
using Common.BasePhrases;

namespace Common.BaseExtensions.ValueTypes;

public static class IntExtensions
{
    /// <summary>
    /// Попытаться преобразовать строку в целое число.
    /// </summary>
    /// <param name="stringValue">Строка для преобразования.</param>
    /// <param name="result">Полученное целое число.</param>
    /// <param name="defaultValue">Значение результата в случае ошибки преобразования.</param>
    public static bool TryParseInt(this string stringValue, out int result, int defaultValue = 0)
    {
        var isOk = int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        if (!isOk)
            result = defaultValue;
        return isOk;
    }

    /// <summary>
    /// Преобразование строки в целое число. 
    /// </summary>
    /// <param name="stringValue">Исходная строка.</param>
    /// <param name="isThrowException">Признак генерации исключения при неудачном преобразовании.</param>
    /// <param name="defaultValue">Значение результата в случае ошибки преобразования.</param>
    public static int ToInt(this string stringValue, bool isThrowException = false, int defaultValue = 0)
    {
        CommonPhrases.Culture = CultureInfo.CurrentUICulture;       // устанавливаем яз. стандарт для фраз

        if (stringValue.TryParseInt(out var result, defaultValue))
            return result;
        
        if (isThrowException)
            throw new FormatException(CommonPhrases.Exception_ParamIsNotNumber.Format(stringValue));
            
        return defaultValue;
    }
}