using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;
using DialogueForest.ViewModels;

namespace DialogueForest.Core.ViewModels
{

    public partial class SettingsViewModel : ObservableObject
    {
        private IApplicationStorageService _applicationStorageService;
        private IInteropService _interop;
        private ForestDataService _dataService;

        private bool _hasInstanceBeenInitialized;

        public SettingsViewModel(IApplicationStorageService appStorage, IInteropService interop, ForestDataService dataService)
        {
            _applicationStorageService = appStorage;
            _interop = interop;
            _dataService = dataService;

            PropertyChanged += SaveSettings;
            ForestMetadata.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MetadataCount));
            ForestCharacters.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterCount));
        }

        private void SaveSettings(object sender, PropertyChangedEventArgs e)
        {
            _applicationStorageService.SetValue(nameof(ElementTheme), _elementTheme.ToString());
            _applicationStorageService.SetValue(nameof(IsCompactSizing), _isCompactSizing);
            _applicationStorageService.SetValue(nameof(EnableAnalytics), _enableAnalytics);
        }

        private void SaveForestSettings(object sender, PropertyChangedEventArgs e)
        {
            var data = new Dictionary<string, MetadataKind>();

            foreach (var vm in ForestMetadata)
            {
                if (vm.Name != null)
                    data.Add(vm.Name, vm.Kind);
            }
            _dataService.SetMetadataDefinitions(data);
        }

        [ObservableProperty]
        private Theme _elementTheme;
        [ObservableProperty]
        private bool _isCompactSizing;
        [ObservableProperty]
        private bool _enableAnalytics;
        [ObservableProperty]
        private string _versionDescription;

        public ObservableCollection<MetadataViewModel> ForestMetadata { get; } = new ObservableCollection<MetadataViewModel>();
        public ObservableCollection<string> ForestCharacters { get; } = new ObservableCollection<string>();

        public int MetadataCount => ForestMetadata.Count;
        public int CharacterCount => ForestCharacters.Count;

        [ICommand]
        private void AddMetadata()
        {
            var vm = new MetadataViewModel { Kind = MetadataKind.STRING };
            vm.PropertyChanged += SaveForestSettings;
            ForestMetadata.Add(vm);
        }

        public void RemoveMetadata()
        {

        }

        [ICommand]
        private async Task SwitchTheme()
        {
            if (_hasInstanceBeenInitialized)
            {
                await _interop.SetThemeAsync(_elementTheme);
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

                LoadCurrentForestSettings();
                _hasInstanceBeenInitialized = true;
            }
        }

        private void LoadCurrentForestSettings()
        {
            foreach (var vm in ForestMetadata)
            {
                vm.PropertyChanged -= SaveForestSettings;
                ForestMetadata.Remove(vm);
            }

            var metadata = _dataService.GetMetadataDefinitions();
            foreach (var key in metadata.Keys)
            {
                var vm = new MetadataViewModel { Name = key, Kind = metadata[key] };
                vm.PropertyChanged += SaveForestSettings;
                ForestMetadata.Add(vm);
            }

            ForestCharacters.Clear();
            foreach (var s in _dataService.GetCharacters())
                ForestCharacters.Add(s);
        }

        private string GetVersionDescription()
        {
            var appName = Resources.AppDisplayName;
            Version version = _interop.GetAppVersion();

            return $"{appName} - {version.Major}.{version.Minor}.{(version.Build > -1 ? version.Build : 0)}.{(version.Revision > -1 ? version.Revision : 0)}";
        }
    }
}
