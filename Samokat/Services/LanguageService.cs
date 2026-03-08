 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Samokat.Utils;

namespace Samokat.Services
{
    public class LanguageService
    {
        private const string LanguageKey = "AppLanguage";
        private const string DefaultLanguage = AppConstants.UZ;

        private readonly AppStoreService _appStore;

        public LanguageService(AppStoreService appStore)
        {
            _appStore = appStore;
        }

        public void Init()
        {
            var isFirstRun = _appStore.Get<bool>(AppConstants.FirstRunKey, true);
            if (isFirstRun)
            {
                _appStore.Set(LanguageKey, DefaultLanguage);
                _appStore.Set(AppConstants.FirstRunKey, false);
                SetCulture(DefaultLanguage);
                return;
            }

            var lang = _appStore.Get<string>(LanguageKey, DefaultLanguage);
            SetCulture(lang);
        }

        public void SetCulture(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            //AppResource.Culture = culture;

            _appStore.Set(LanguageKey, cultureCode);
        }

        public string GetCurrentLanguage()
        {
            return _appStore.Get<string>(LanguageKey, DefaultLanguage);
        }

        public string GetString(string key)
        {
            return ""; //AppResource.ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
        }
    }
}
