using System.Globalization;
using AppDomain.Setting;
using AppDomain.Setting.Entities;
using AppDomain.Setting.Services;

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
        // TODO: сделать копирование файла конфигурации в тесты после компиляции
        
        var appSetting = new AppSettingService();
        var appLocalization = appSetting.AppLocalization;

        var lang1 = appLocalization.GetCurrentLang();
        var currLang = appLocalization.GetCurrentOrDefaultLang();
        var langName1 = appLocalization.GetLangFromSetting();
        var lang2 = appLocalization.SetCurrentLangFromName(langName1);
        var lang3 = appLocalization.SetCurrentLangFromName("langName1");
        var lang4 = appLocalization.GetCurrentLang();

        var lang5 = currLang.Clone();
        lang5.Translate(CultureInfo.GetCultureInfo("fr-FR"));
    }
}