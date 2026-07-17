using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions.ValueTypes;

namespace Common.Tests;

public class BaseExtensionsTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    public static bool IsNullableType<T>(T value)
    {
        // Проверяем, является ли T nullable-типом (значимым)
        return Nullable.GetUnderlyingType(typeof(T)) != null;
    }

    [Test]
    public void EnumTest()
    {
        var int1 = 2;
        var enm1 = int1.ToEnum<ExcptnTypeEnm>();
        Assert.That(enm1, Is.EqualTo(ExcptnTypeEnm.Warning),
            "Значение перечисления должно быть равно ExcptnTypeEnm.Warning.");;

        ExcptnTypeEnm? enm2 = enm1;
        var enm3 = enm2.FromNullable();
        Assert.That(IsNullableType(enm3), Is.Not.True,
            "Тип переменной не должен быть nulleble.");;
    }
}