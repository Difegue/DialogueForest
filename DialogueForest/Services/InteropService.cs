﻿using System;
using System.Threading.Tasks;

using DialogueForest.Core.Interfaces;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using DialogueForest.Core.ViewModels;
using System.IO;
using Windows.ApplicationModel;
using CommunityToolkit.WinUI.Helpers;
using DialogueForest.Helpers;
using Windows.UI.Core;
using WinUIEx;
using Microsoft.UI.Xaml.Controls.Primitives;

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
            return new Version(1,0); // TODO
        }

        private async Task SetRequestedThemeAsync(ElementTheme theme)
        {
            await _dispatcherService.ExecuteOnUIThreadAsync(() =>
            {
                if ((Application.Current as App)?.Window.WindowContent is FrameworkElement frameworkElement)
                {
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

            var titleBar = (Application.Current as App)?.Window.GetAppWindow().TitleBar;
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
            //await _dispatcherService.ExecuteOnUIThreadAsync(() => SystemInformation.LaunchStoreForReviewAsync());
        }

        public void UpdateAppTitle(string title)
        {
            if ((Application.Current as App).Window != null)
                (Application.Current as App).Window.Title = title;
        }
    }
}
