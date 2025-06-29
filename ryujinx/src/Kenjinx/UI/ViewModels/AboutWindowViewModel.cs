using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.UI.Common.Configuration;
using System;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class AboutWindowViewModel : BaseModel, IDisposable
    {
        private Bitmap _gitlabLogo;
        private Bitmap _discordLogo;

        private string _version;

        public Bitmap GitLabLogo
        {
            get => _gitlabLogo;
            set
            {
                _gitlabLogo = value;
                OnPropertyChanged();
            }
        }

        public Bitmap DiscordLogo
        {
            get => _discordLogo;
            set
            {
                _discordLogo = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }

        public string FormerDevelopers => LocaleManager.Instance.UpdateAndGetDynamicValue(LocaleKeys.AboutPageDeveloperListMore, "gdkchan, Ac_K, marysaka, rip in peri peri, LDj3SNuD, emmaus, Thealexbarney, GoffyDude, TSRBerry, IsaacMarovitz");

        public string Developers => "KeatonTheBot";
        
        public AboutWindowViewModel()
        {
            Version = Program.Version;
            UpdateLogoTheme(ConfigurationState.Instance.UI.BaseStyle.Value);

            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() => UpdateLogoTheme(ConfigurationState.Instance.UI.BaseStyle.Value));
        }

        private void UpdateLogoTheme(string theme)
        {
            bool isDarkTheme = theme == "Dark" || (theme == "Auto" && App.DetectSystemTheme() == ThemeVariant.Dark);

            string basePath = "resm:Ryujinx.UI.Common.Resources.";
            string themeSuffix = isDarkTheme ? "Dark.png" : "Light.png";

            GitLabLogo = LoadBitmap($"{basePath}Logo_GitLab_{themeSuffix}?assembly=Ryujinx.UI.Common");
            DiscordLogo = LoadBitmap($"{basePath}Logo_Discord_{themeSuffix}?assembly=Ryujinx.UI.Common");
        }

        private Bitmap LoadBitmap(string uri)
        {
            return new Bitmap(Avalonia.Platform.AssetLoader.Open(new Uri(uri)));
        }

        public void Dispose()
        {
            ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            GC.SuppressFinalize(this);
        }
    }
}
