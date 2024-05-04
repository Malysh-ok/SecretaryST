using System.Globalization;
using AppDomain.AppExceptions;
using Common.BaseComponents.Components.Exceptions;
using Common.BaseExtensions;
using Common.BaseExtensions.Collections;
using Common.Phrases;
using DataAccess.Repositories.Exceptions;
using ProblemDomain.Entities._Contracts;
using ProblemDomain.Entities.CommonEntities;

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
        
        var ex11 = BaseException.CreateException<SystemException>(
            "Exception...", null, "ru", "Исключение");
        var ex12 =  BaseException.CreateException<BaseException>(
            "Exception", null, "ru", "Исключение");
        var ex13 =  BaseException.CreateException<AppException>(
            "Exception", null, "ru", "Исключение");
        var ex14 =  BaseException.CreateException<DbException>(
            "Exception", null, "ru", "Исключение");
        
        // ------------------------
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru");
        
        var ex21 = BaseException.CreateException<SystemException>(
            "Exception", null, "ru", "Исключение");
        var ex22 =  BaseException.CreateException<BaseException>(
            "Exception", null, "ru", "Исключение");
        var ex23 =  BaseException.CreateException<AppException>(
            "Exception", null, "ru", "Исключение");
        var ex24 =  BaseException.CreateException<DbException>(
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
    }

    [Test]
    public void ExceptionTest()
    {
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en");
        var be11 = new BaseException();
        var be12 = new BaseException("Два");
        var be13 = new BaseException("Check.", new Exception("Проверка - Inner."),
            "ru", "Проверка.");

        /*
        var m1 = BaseException.ConvertMessage(typeof(Exception), 
            DbPhrases.DbСonnectiontError, "en");
        var m2 = BaseException.ConvertMessage(typeof(DbPhrases), 
            DbPhrases.DbСonnectiontError, "ru");
        var m3 = BaseException.ConvertMessage(typeof(DbPhrases), 
            "Ла-ла-ла", "ru");
        var m4 = BaseException.ConvertMessage(typeof(DbPhrases), 
            null, "ru");
        */

        var dfe11 = new DbFatalException();
        var dfe12 = new DbFatalException("Fatal Exception");
        
        var dfe13 = new DbFatalException("Fatal Exception", 
            null, "ru", "Фатальное исключение");
        var dfe14 = new DbFatalException(innerException: 
            new Exception("Проверка - Inner"));
        
        // ------------------------
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ru");
        
        var be21 = new BaseException("Check.", new Exception("Проверка - Inner."),
            "ru", "Проверка.");
        
        var dfe21 = new DbFatalException();
        var dfe22 = new DbFatalException("Fatal Exception", 
            null, "ru", "Фатальное исключение");
        var dfe23 = new DbFatalException(innerException: 
            new Exception("Проверка - Inner"));

        var result1 = be11.Message.IsOnlyLatinChars() &&
                      ! be12.Message.IsOnlyLatinChars() &&
                      be13.Message.IsOnlyLatinChars() &&
                      dfe11.Message.IsOnlyLatinChars() &&
                      dfe12.Message.IsOnlyLatinChars() &&
                      dfe13.Message.IsOnlyLatinChars() &&
                      dfe14.Message.IsOnlyLatinChars();
        Assert.That(result1, Is.True, "Ошибка при работе с исключениями.");

        var result2 = be21.Message.IsOnlyCyrillicChars() &&
                      dfe21.Message.IsOnlyCyrillicChars() &&
                      dfe22.Message.IsOnlyCyrillicChars() &&
                      dfe23.Message.IsOnlyCyrillicChars();
        Assert.That(result2, Is.True, "Ошибка при работе с исключениями.");

    }

    [Test]
    public void TmpTest()
    {
        var representative = new Representative("Банько", "Яна", "Евгеньевна");
        
        // Делегации
        var delegations = new List<Delegation>()
        {
            new(1, name: "Делегация 11111", "Якутия", representative),
            new(2, name: "Делегация 22222", "Сахалин", representative),
        };

        var newList = delegations.DeepCopy();
        ICopy delegation = delegations[0];
        delegation.Copy(delegations[1]);
    }
}