using System;
using System.Threading.Tasks;

using DialogueForest.Core.Interfaces;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using System.IO;
using System.Reflection;

namespace DialogueForest.Services
{
    public class InteropService : IInteropService
    {
        private ApplicationTheme _appTheme;
        private IDispatcherService _dispatcherService;

        public InteropService(IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
            
            UISettings uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += HandleSystemThemeChange;
        }

        private void HandleSystemThemeChange(UISettings sender, object args)
        {
            if ((Application.Current as App)?.Window.WindowContent is FrameworkElement frameworkElement)
            {
                UpdateTitleBar(frameworkElement.RequestedTheme);
            }
        }

        public async Task SetThemeAsync(Theme theme)
        {
            await SetRequestedThemeAsync(GetTheme(theme));
        }

        public Version GetAppVersion()
        {
            // Return the Assembly version.
            var assembly = Assembly.GetExecutingAssembly();
            return AssemblyName.GetAssemblyName(assembly.Location).Version ?? new Version(1, 0);
        }

        private async Task SetRequestedThemeAsync(ElementTheme theme)
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                if ((Application.Current as App)?.Window.WindowContent is FrameworkElement frameworkElement)
                {
                    // TODO: Settings Default here after having changed the RequestedTheme once doesn't work. (I blame winappsdk)
                    frameworkElement.RequestedTheme = theme;
                    UpdateTitleBar(theme);
                }
            });
        }

        private void UpdateTitleBar(ElementTheme theme)
        {
            // https://stackoverflow.com/questions/48201278/uwp-changing-titlebar-buttonforegroundcolor-with-themeresource
            Color? color = null;
            _appTheme = Application.Current.RequestedTheme;

            switch (theme)
            {
                case ElementTheme.Default:
                    color = (Color)Application.Current.Resources["SystemBaseHighColor"];
                    break;
                case ElementTheme.Light:
                    if (_appTheme == ApplicationTheme.Light) { color = ((Color)Application.Current.Resources["SystemBaseHighColor"]); }
                    else { color = (Color)Application.Current.Resources["SystemAltHighColor"]; }
                    break;
                case ElementTheme.Dark:
                    if (_appTheme == ApplicationTheme.Light) { color = ((Color)Application.Current.Resources["SystemAltHighColor"]); }
                    else { color = (Color)Application.Current.Resources["SystemBaseHighColor"]; }
                    break;
                default:
                    break;
            }

            var titleBar = (Application.Current as App)?.Window.AppWindow.TitleBar;
            titleBar.ButtonForegroundColor = color;
        }

        private ElementTheme GetTheme(Theme theme)
        {
            return theme switch
            {
                Theme.Default => ElementTheme.Default,
                Theme.Light => ElementTheme.Light,
                Theme.Dark => ElementTheme.Dark,
                _ => throw new NotImplementedException(),
            };
        }

        public async Task OpenStoreReviewUrlAsync()
        {
            // Open url
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://difegue.itch.io/dialogueforest"));

            // TODO packaged ver
            //await _dispatcherService.ExecuteOnUIThreadAsync(() => SystemInformation.LaunchStoreForReviewAsync());
        }

        public void UpdateAppTitle(string title)
        {
            if ((Application.Current as App).Window != null)
                (Application.Current as App).Window.Title = title;
        }

        public DateTime GetLastModifiedTime(FileAbstraction lastSavedFile)
        {
            var filePath = lastSavedFile?.FullPath;

            if (string.IsNullOrEmpty(filePath))
                return DateTime.MinValue;
            else
                return File.GetLastWriteTime(filePath);
        }
    }
}
