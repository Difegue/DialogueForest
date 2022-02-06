using System;
using System.Threading.Tasks;

using DialogueForest.Core.Interfaces;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using SkiaSharp;
using DialogueForest.Core.ViewModels;
using System.IO;
using Windows.ApplicationModel;
using Microsoft.Toolkit.Uwp.Helpers;
using DialogueForest.Helpers;

namespace DialogueForest.Services
{
    public class InteropService : IInteropService
    {
        private ApplicationTheme _appTheme;

        public InteropService()
        {

            UISettings uiSettings = new UISettings();
            uiSettings.ColorValuesChanged += HandleSystemThemeChange;

            // Fallback in case the above fails, we'll check when we get activated next.
            Window.Current.CoreWindow.Activated += CoreWindow_Activated;
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (Window.Current.Content is FrameworkElement frameworkElement && _appTheme != Application.Current.RequestedTheme)
            {
                UpdateTitleBar(frameworkElement.RequestedTheme);
            }
        }

        private void HandleSystemThemeChange(UISettings sender, object args)
        {
            if (Window.Current?.Content is FrameworkElement frameworkElement)
            {
                UpdateTitleBar(frameworkElement.RequestedTheme);
            }  
        }

        public async Task SetThemeAsync(Theme theme)
        {
            await SetRequestedThemeAsync(GetTheme(theme));
        }

        public SKColor GetAccentColor()
        {
            var accent = (Color)Application.Current.Resources["SystemAccentColor"];

            return new SKColor(accent.R, accent.G, accent.B, accent.A);
        }

        public Version GetAppVersion()
        {
            var package = Package.Current;
            var packageId = package.Id;
            return new Version(packageId.Version.Major, packageId.Version.Minor, packageId.Version.Revision, packageId.Version.Build);
        }

        private async Task SetRequestedThemeAsync(ElementTheme theme)
        {
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = theme;
                        UpdateTitleBar(theme);
                    }
                });
            }
        }
        private void UpdateTitleBar(ElementTheme theme)
        {
            // https://stackoverflow.com/questions/48201278/uwp-changing-titlebar-buttonforegroundcolor-with-themeresource
            Color color;
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

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
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
            await CoreApplication.GetCurrentView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SystemInformation.LaunchStoreForReviewAsync());
        }

        public string ConvertRtf(string rtf, OutputFormat format) => RtfHelper.Convert(rtf, format);
    }
}
