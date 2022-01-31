using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{

    public partial class SettingsViewModel : ObservableObject
    {
        private IApplicationStorageService _applicationStorageService;
        private IInteropService _interop;

        private bool _hasInstanceBeenInitialized;

        public SettingsViewModel(IApplicationStorageService appStorage, IInteropService interop)
        {
            _applicationStorageService = appStorage;
            _interop = interop;

            PropertyChanged += SaveSettings;
        }

        private void SaveSettings(object sender, PropertyChangedEventArgs e)
        {
            _applicationStorageService.SetValue(nameof(ElementTheme), _elementTheme.ToString());
            _applicationStorageService.SetValue(nameof(IsCompactSizing), _isCompactSizing);
            _applicationStorageService.SetValue(nameof(EnableAnalytics), _enableAnalytics);
        }

        [ObservableProperty]
        private Theme _elementTheme;
        [ObservableProperty]
        private bool _isCompactSizing;
        [ObservableProperty]
        private bool _enableAnalytics;
        [ObservableProperty]
        private string _versionDescription;

        [ICommand]
        private async Task SwitchTheme(Theme param)
        {
            if (_hasInstanceBeenInitialized)
            {
                ElementTheme = param;
                await _interop.SetThemeAsync(param);
            }
        }

        [ICommand]
        private void SwitchSizing(string param)
        {
            if (_hasInstanceBeenInitialized)
            {
                IsCompactSizing = bool.Parse(param);
            }
        }

        [ICommand]
        private async Task RateApp()
        {
            await _interop.OpenStoreReviewUrlAsync();
        }

        public void EnsureInstanceInitialized()
        {
            if (!_hasInstanceBeenInitialized)
            {
                // Initialize values directly 
                _isCompactSizing = _applicationStorageService.GetValue<bool>(nameof(IsCompactSizing));
                _enableAnalytics = _applicationStorageService.GetValue(nameof(EnableAnalytics), true);

                Enum.TryParse(_applicationStorageService.GetValue<string>(nameof(ElementTheme)), out _elementTheme);

                VersionDescription = GetVersionDescription();
                _hasInstanceBeenInitialized = true;
            }
        }

        private string GetVersionDescription()
        {
            var appName = Resources.AppDisplayName;
            Version version = _interop.GetAppVersion();

            return $"{appName} - {version.Major}.{version.Minor}.{(version.Build > -1 ? version.Build : 0)}.{(version.Revision > -1 ? version.Revision : 0)}";
        }
    }
}
