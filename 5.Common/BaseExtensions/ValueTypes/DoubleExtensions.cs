using System;
using System.Globalization;
using Common.BasePhrases;

namespace Common.BaseExtensions.ValueTypes;

/// <summary>
/// Методы расширения для <see cref="Double" />.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Формат вещественных чисел с разделителем ".".
    /// </summary>
    public static NumberFormatInfo NfiDot => 
        (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();

    /// <summary>
    /// Формат вещественных чисел с разделителем ",".
    /// </summary>
    public static NumberFormatInfo NfiComma
    {
        get
        {
            var nfiRus = (NumberFormatInfo)CultureInfo.GetCultureInfo("ru-RU").NumberFormat.Clone();
            nfiRus.CurrencyDecimalSeparator = ",";
            nfiRus.CurrencyGroupSeparator = " ";
            return nfiRus;
        }
    }

    /// <summary>
    /// Попытаться преобразовать строку в вещественное число.
    /// </summary>
    /// <remarks>
    /// Метод работает с разделителями целой части '.' и ','.
    /// Если преобразование невозможно, - result = 0.
    /// </remarks>
    /// <param name="stringValue">Строка для преобразования.</param>
    /// <param name="result">Полученное вещественное число.</param>
    /// <param name="defaultValue">Значение результата в случае ошибки преобразования.</param>
    public static bool TryParseDouble(this string stringValue, out double result, double defaultValue = double.NaN)
    {
        if (! stringValue.IsNullOrWhiteSpace())
            return double.TryParse(stringValue, NumberStyles.Float, 
                stringValue.Contains(".") ? NfiDot : NfiComma, out result);

        result = defaultValue;
        return false;
    }

    /// <summary>
    /// Преобразование строки в вещественное число.
    /// </summary>
    /// <remarks>
    /// Метод работает с разделителями целой части '.' и ','.
    /// Если преобразование невозможно, - возвращается double.NaN.
    /// </remarks>
    /// <param name="stringValue">Строка для преобразования.</param>
    /// <param name="isThrowException">Признак генерации исключения при неудачном преобразовании.</param>
    /// <param name="defaultValue">Значение результата в случае ошибки преобразования (если <paramref name="isThrowException"/>=false).</param>
    public static double ToDouble(this string stringValue, bool isThrowException = false, double defaultValue = double.NaN)
    {
        CommonPhrases.Culture = CultureInfo.CurrentUICulture;       // устанавливаем яз. стандарт для фраз
            
        if (stringValue.TryParseDouble(out var result))
            return result;

        if (isThrowException)
            throw new FormatException(CommonPhrases.Exception_ParamIsNotNumber.Format(stringValue));
            
        return defaultValue;
    }

    /// <summary>
    /// Проверка вхождения числа <paramref name="number"/> в диапазон от lower до upper.
    /// </summary>
    /// <remarks> Если параметр inclusive = true, - сравнение с конечными значениями включительно. </remarks>
    public static bool IsBetween(this double number, double lower, double upper, bool inclusive = false)
    {
        return inclusive
            ? lower <= number && number <= upper
            : lower < number && number < upper;
    }
}