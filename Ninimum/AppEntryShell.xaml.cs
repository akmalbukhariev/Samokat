using Ninimum.Services;
using Ninimum.Views.LoginRegister;
using Ninimum.Views.Startup;

namespace Ninimum
{
    public partial class AppEntryShell : Shell
    {
        private AppStoreService appStoreService;
        private LanguageService languageService;
        public AppEntryShell()
        {
            InitializeComponent();

            appStoreService = AppService.Get<AppStoreService>();
            languageService = AppService.Get<LanguageService>();

            Init();
        }

        private void Init()
        {
            Items.Clear();

            //languageService.SetCulture(languageService.GetCurrentLanguage());

            //bool isLanguageSet = appStoreService.Get(AppKeys.IsLanguageSet, false);
            //if (isLanguageSet)
            {
                var loginItem = new ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(LoginPage))
                    //ContentTemplate = new DataTemplate(typeof(BlockedPage))
                };

                Items.Add(loginItem);
            }
            /*else
            {
                var languageItem = new ShellContent
                {
                    ContentTemplate = new DataTemplate(typeof(StartPage))
                };

                Items.Add(languageItem);
            }*/
        }
    }
}
