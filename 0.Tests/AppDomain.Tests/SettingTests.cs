using System.Globalization;
using AppDomain.AppAssets.Services;
using AppDomain.AppEntities;
using AppDomain.AppUseCases._Contracts;
using Presentation.Shell.Common;

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
        var testAppInfo = ServiceFactory.CreateAppInfo("Test", new Version(1,0,0,0), DateTime.Now);
        var appDirService = ServiceFactory.CreateAppDirService(appErrorMsgProvider);
        var appSettingService = ServiceFactory.CreateAppSettingService(appErrorMsgProvider, appDirService, testAppInfo);
        var appLocalization = appSettingService.AppLocalization;

        var lang1 = appLocalization.GetCurrentLang();
        var currLang = appLocalization.GetCurrentOrDefaultLang();
        var langName1 = appLocalization.GetLangFromSetting();
        var lang2 = appLocalization.SetCurrentLangFromName(langName1);
        var lang3 = appLocalization.SetCurrentLangFromName("langName1");
        var lang4 = appLocalization.GetCurrentLang();

        var lang5 = currLang.Clone();
        lang5.Translate(CultureInfo.GetCultureInfo("ru-RU"));
        var ci = CultureInfo.GetCultureInfo("ru-RU");
        
        var lang6 = appLocalization.GetFromNameOrDefault("ru-RU");
        // appLocalization.SetLangToSetting(lang6.Name);
        // appSetting.SaveConfig();

        var lang7 = new Lang(CultureInfo.GetCultureInfo("ru-RU"), CultureInfo.GetCultureInfo("en-EN"));
        lang7.Translate(CultureInfo.GetCultureInfo("ru-RU"));
    }
}