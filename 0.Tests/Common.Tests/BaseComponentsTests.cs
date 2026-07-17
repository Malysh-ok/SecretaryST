using System.Globalization;
using AppDomain.AppAssets.Services;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Common.BaseComponents.Components;
using Common.BaseComponents.Components.Colors;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using DataAccess.DataAccessExceptions;
using ProblemDomain.ProblemExceptions;
using ProblemDomain.UseCases._Contracts;

namespace Common.Tests;

public class BaseComponentsTests
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void BaseExceptionTest()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
        
        var ex11 = BaseException.Create<SystemException>(
            "Exception...", null, "ru", "Исключение");
        var ex12 =  BaseException.Create<BaseException>(
            "Exception", null, "ru", "Исключение");
        var ex13 =  BaseException.Create<AppException>(
            "Exception", null, "ru", "Исключение");
        var ex14 =  BaseException.Create<DataAccessException>(
            "Exception", null, "ru", "Исключение");
        
        // ------------------------
        
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru");
        
        var ex21 = BaseException.Create<SystemException>(
            "Exception", null, "ru", "Исключение");
        var ex22 =  BaseException.Create<BaseException>(
            "Exception", null, "ru", "Исключение");
        var ex23 =  BaseException.Create<AppException>(
            "Exception", null, "ru", "Исключение");
        var ex24 =  BaseException.Create<DataAccessException>(
            "Exception", null, "ru", "Исключение");

        var result1 = ex11.Message.IsOnlyLatinChars() &&
                      ex12.Message.IsOnlyLatinChars() &&
                      ex13.Message.IsOnlyLatinChars() &&
                      ex14.Message.IsOnlyLatinChars();
        Assert.That(result1, Is.True, "Ошибка при работе с исключениями.");
        
        var result2 = ex21.Message.IsOnlyCyrillicChars() &&
                      ex22.Message.IsOnlyCyrillicChars() &&
                      ex23.Message.IsOnlyCyrillicChars() &&
                      ex24.Message.IsOnlyCyrillicChars();
        Assert.That(result2, Is.True, "Ошибка при работе с исключениями.");
        
        // ------------------------

        var ex31 = new BaseException("Exception"){ ExcptnType = ExcptnTypeEnm.Warning};
        Assert.That(ex31.ExcptnType, Is.EqualTo(ExcptnTypeEnm.Warning), 
            "Тип исключения должен быть ExcptnTypeEnm.Warning.");
        
        ex31.ResetExcptnType(ExcptnTypeEnm.Info);
        Assert.That(ex31.ExcptnType, Is.EqualTo(ExcptnTypeEnm.Info), 
            "Тип исключения должен быть ExcptnTypeEnm.Info.");
    }

    [Test]
    public void AppAndProblemExceptionTest()
    {
        IProblemErrorMsgProvider problemErrorMsgProvider = new DomainErrorMsgProvider();
        IAppErrorMsgProvider appErrorMsgProvider = new DomainErrorMsgProvider();

        // var innerEx1 = ProblemException.CreateFromErrorCode(ProblemErrorCodes.SportEventCreateError);
        var innerEx1 = problemErrorMsgProvider.CreateException(ProblemErrorCodes.SportEventCreateError);
        var innerEx2 = ProblemException.CreateFromErrorCode("0", innerEx1, "=== innerEx2 ===");
        // var innerEx3 = AppException.CreateFromErrorCode(ProblemErrorCodes.CompetitionDataCreateError,  innerEx2);
        var innerEx3 = appErrorMsgProvider.CreateException(AppErrorCodes.LocalizingError, innerEx2, args: "TestView");
        var ex = AppException.CreateFromErrorCode(AppErrorCodes.UnknownError, innerEx3, "Верхнее исключение");

        Exception? currEx = ex;
        do
        {
            Assert.That(currEx.Message, Does.Not.StartWith("Exception of type"), 
                "Одно из внутренних исключений содержит код ошибки, но не содержит соответствующего ей сообщения.");
            currEx = currEx.InnerException;
        } while (currEx != null);
    }

    [Test]
    public void HsbColorTest()
    {
        var hsbColor1 = HsbColor.FromAhsb(100, 180, 0.5f, 0.5f);
        var hsbColor2 = HsbColor.FromAhsb(100, 180, 0.5f, 0.5f);
        
        Assert.That(hsbColor1 == hsbColor2, Is.True, "Цвета должны быть равны.");
        Assert.That(hsbColor1 != hsbColor2, Is.False, "Цвета должны быть равны.");

        hsbColor2.Alpha = 0;
        Assert.That(hsbColor1.EqualsWithoutAlpha(hsbColor2), Is.True, "Цвета должны быть равны.");
        Assert.That(hsbColor1 == hsbColor2, Is.False, "Цвета НЕ должны быть равны.");
        Assert.That(hsbColor1 != hsbColor2, Is.True, "Цвета НЕ должны быть равны.");
    }

    [Test]
    public void TmpTest()
    {
    }

    [Test]
    public void ResultTest()
    {
        var result = Result<object?>.Done(null);
        Assert.That(result.Excptn, Is.Null);
    }
}