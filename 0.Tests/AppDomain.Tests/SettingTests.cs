using System.Globalization;
using AppDomain.AppAssets.Services;
using AppDomain.AppEntities;
using AppDomain.AppExceptions;
using AppDomain.AppUseCases._Contracts;
using Presentation.Shell.Infrastructure;

namespace AppDomain.Tests;

public class SettingTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TmpTest()
    {
    }

    [Test]
    public void LocalizationTest()
    {
        IAppErrorMsgProvider appErrorMsgProvider = new DomainErrorMsgProvider();
        IEmbeddedResourceProvider embeddedResourceProvider = new EmbeddedResourceProvider();
        var testAppInfo = ServiceFactory.CreateAppInfo("Test", new Version(1,0,0,0), DateTime.Now);
        var appDirService = ServiceFactory.CreateAppDirService(appErrorMsgProvider);
        var appSettingService = ServiceFactory.CreateAppSettingService(
            appErrorMsgProvider, embeddedResourceProvider, appDirService, testAppInfo);
        var appLocalization = appSettingService.AppLocalization;
        Assert.That(appLocalization.InitializationException, Is.Null, 
            "Исключение должно отсутствовать.");

        {
            var isOk1 = appLocalization.ValidateLang(null);
            Assert.That(isOk1, Is.False,
                "Должно быть False.");

            var isOk2 = appLocalization.ValidateLangName(null);
            Assert.That(isOk2, Is.False,
                "Должно быть False.");

            var isOk3 = appLocalization.ValidateLangName("ru-RU");
            Assert.That(isOk3, Is.True,
                "Должно быть True.");
        }

        {
            var langResult = appLocalization.GetLangFromName("fr-FR");

            var lang1 = langResult.Value?.Clone();
            Assert.That(lang1!.DisplayName, Is.EqualTo("French"), 
                "Естественное название языка должно быть 'French'.");
            lang1.Translate(CultureInfo.GetCultureInfo("ru-RU"));
            Assert.That(lang1.DisplayName, Is.EqualTo("Французский"), 
                "Естественное название языка должно быть 'Французский'.");

            var lang2 = new Lang(CultureInfo.GetCultureInfo("ru-RU"), CultureInfo.GetCultureInfo("en-EN"));
            Assert.That(lang2.DisplayName, Is.EqualTo("Russian"), 
                "Естественное название языка должно быть 'Russian'.");
            lang2.Translate(CultureInfo.GetCultureInfo("ru-RU"));
            Assert.That(lang2.DisplayName, Is.EqualTo("Русский"), 
                "Естественное название языка должно быть 'Русский'.");
        }

        {
            var currLang = appLocalization.CurrentLang;
            
            var newLang = new Lang(new CultureInfo("sa-IN"));
            var langResult = appLocalization.SetCurrentLang(newLang);
            var appEx = langResult.Excptn as AppException;
            Assert.That(appEx, Is.Not.Null, 
                "Должно быть исключение.");
            Assert.That(appEx.ErrCode, Is.EqualTo(AppErrorCodes.LanguageNotFound), 
                "Должно быть исключение с кодом AppErrorCodes.LanguageNotFound.");
            
            var currLang2 = appLocalization.CurrentLang;
            Assert.That(currLang, Is.EqualTo(currLang2), 
                "Переменная currLang2 должен быть равна currLang.");
        }

        {
            var langResult = appLocalization.GetLangFromName("fr-FR");
            Assert.That(langResult.Excptn, Is.Null, 
                "Исключение должно отсутствовать.");

            var langResult2 = appLocalization.GetLangFromName("sa-IN");
            var appEx = langResult2.Excptn as AppException;
            Assert.That(appEx, Is.Not.Null, 
                "Должно быть исключение.");
            Assert.That(appEx.ErrCode, Is.EqualTo(AppErrorCodes.LanguageNotFound), 
                "Должно быть исключение с кодом AppErrorCodes.LanguageNotFound.");

            var lang = appLocalization.GetLangFromNameOrDefault("langName1");
            Assert.That(lang, Is.EqualTo(appLocalization.DefaultLang), 
                "Переменная lang должен быть равна текущему языку.");
        }

        {
            var langResult = appLocalization.GetLangFromName("ru-RU");
            var langResult1 = appLocalization.SetCurrentLang(langResult.Value);
            Assert.That(langResult1.Excptn, Is.Null, 
                "Исключение должно отсутствовать.");
            Assert.That(appLocalization.CurrentLang, Is.EqualTo(langResult.Value), 
                "Текущий язык должен равняться 'ru-RU'.");
            
            langResult = appLocalization.GetLangFromName("en-US");
            var langResult2 = appLocalization.SetCurrentLang(langResult.Value);
            Assert.That(langResult2.Excptn, Is.Null, 
                "Исключение должно отсутствовать.");
            Assert.That(appLocalization.CurrentLang, Is.EqualTo(langResult.Value), 
                "Текущий язык должен равняться 'en-US'.");
        }
    }
}