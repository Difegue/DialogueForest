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
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{

    public partial class SettingsViewModel : ObservableObject
    {
        public const int DEFAULT_WORD_OBJECTIVE = 200;

        private IApplicationStorageService _applicationStorageService;
        private IInteropService _interop;
        private INotificationService _notificationService;
        private ForestDataService _dataService;

        private bool _hasInstanceBeenInitialized;

        public SettingsViewModel(IApplicationStorageService appStorage, IInteropService interop, INotificationService notifications, ForestDataService dataService)
        {
            _applicationStorageService = appStorage;
            _interop = interop;
            _dataService = dataService;
            _notificationService = notifications;

            PropertyChanged += SaveSettings;
            ForestMetadata.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MetadataCount));
            ForestCharacters.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterCount));
        }

        private void SaveSettings(object sender, PropertyChangedEventArgs e)
        {
            _applicationStorageService.SetValue(nameof(ElementTheme), ElementTheme.ToString());
            _applicationStorageService.SetValue(nameof(IsCompactSizing), IsCompactSizing);
            _applicationStorageService.SetValue(nameof(EnableAnalytics), EnableAnalytics);

            _applicationStorageService.SetValue(nameof(EnableWordTracking), EnableWordTracking);
            _applicationStorageService.SetValue(nameof(DailyWordObjective), DailyWordObjective);
            _applicationStorageService.SetValue(nameof(EnableNotification), EnableNotification);
            _applicationStorageService.SetValue(nameof(NotificationTime), NotificationTime.TotalMinutes);

            // Send a message to inform VMs the settings changed
            WeakReferenceMessenger.Default.Send<SettingsChangedMessage>();

            // Update notifications
            if (e.PropertyName == nameof(EnableNotification) || e.PropertyName == nameof(NotificationTime) || e.PropertyName == nameof(EnableWordTracking) || e.PropertyName == nameof(DailyWordObjective))
                UpdateNotifications();
        }

        private void SaveForestSettings(object sender, PropertyChangedEventArgs e)
        {
            var data = new Dictionary<string, MetadataKind>();

            foreach (var vm in ForestMetadata.ToList())
            {
                if (vm.Name != null)
                {
                    // very cheap dupe checker
                    while (data.ContainsKey(vm.Name))
                        vm.Name += "'";

                    data.Add(vm.Name, vm.Kind);
                }
            }
            _dataService.SetMetadataDefinitions(data);
            _dataService.SetCharacters(ForestCharacters.Select(vm => vm.Name).ToList());

            _dataService.SaveForestToStorage();

            // Send a message to inform VMs the settings changed
            WeakReferenceMessenger.Default.Send<ForestSettingsChangedMessage>();
        }

        [ObservableProperty]
        private Theme _elementTheme;
        [ObservableProperty]
        private bool _isCompactSizing;
        [ObservableProperty]
        private bool _enableAnalytics;

        [ObservableProperty]
        private bool _enableWordTracking;
        [ObservableProperty]
        private int _dailyWordObjective;
        [ObservableProperty]
        private bool _enableNotification;
        [ObservableProperty]
        private TimeSpan _notificationTime;

        [ObservableProperty]
        private string _versionDescription;

        public ObservableCollection<MetadataViewModel> ForestMetadata { get; } = new ObservableCollection<MetadataViewModel>();
        public ObservableCollection<CharacterViewModel> ForestCharacters { get; } = new ObservableCollection<CharacterViewModel>();

        public int MetadataCount => ForestMetadata.Count;
        public int CharacterCount => ForestCharacters.Count;

        [RelayCommand]
        private void AddCharacter()
        {
            var vm = new CharacterViewModel(this);
            vm.PropertyChanged += SaveForestSettings;
            ForestCharacters.Add(vm);
        }

        public void RemoveCharacter(CharacterViewModel vm)
        {
            vm.PropertyChanged -= SaveForestSettings;
            ForestCharacters.Remove(vm);
            SaveForestSettings(this, null);
        }

        [RelayCommand]
        private void AddMetadata()
        {
            var vm = new MetadataViewModel(this) { Kind = MetadataKind.STRING };
            vm.PropertyChanged += SaveForestSettings;
            ForestMetadata.Add(vm);
        }

        public void RemoveMetadata(MetadataViewModel vm)
        {
            vm.PropertyChanged -= SaveForestSettings;
            ForestMetadata.Remove(vm);
            SaveForestSettings(this, null);
        }

        [RelayCommand]
        private async Task SwitchTheme()
        {
            if (_hasInstanceBeenInitialized)
            {
                await _interop.SetThemeAsync(_elementTheme);
            }
        }

        [RelayCommand]
        private void SwitchSizing(string param)
        {
            if (_hasInstanceBeenInitialized)
            {
                IsCompactSizing = bool.Parse(param);
            }
        }

        [RelayCommand]
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

                _enableWordTracking = _applicationStorageService.GetValue<bool>(nameof(EnableWordTracking), true);

                var objective = _applicationStorageService.GetValue(nameof(DailyWordObjective), DEFAULT_WORD_OBJECTIVE);
                _dailyWordObjective = objective == 0 ? DEFAULT_WORD_OBJECTIVE : objective;
                _enableNotification = _applicationStorageService.GetValue<bool>(nameof(EnableNotification));

                var time = _applicationStorageService.GetValue(nameof(NotificationTime), 0);
                _notificationTime = TimeSpan.FromMinutes(time);
                
                Enum.TryParse(_applicationStorageService.GetValue<string>(nameof(ElementTheme)), out _elementTheme);

                VersionDescription = GetVersionDescription();

                UpdateNotifications();
                _hasInstanceBeenInitialized = true;
            }
        }

        internal void LoadCurrentForestSettings()
        {
            foreach (var vm in ForestMetadata.ToList())
            {
                vm.PropertyChanged -= SaveForestSettings;
                ForestMetadata.Remove(vm);
            }

            foreach (var vm in ForestCharacters.ToList())
            {
                vm.PropertyChanged -= SaveForestSettings;
                ForestCharacters.Remove(vm);
            }

            var metadata = _dataService.GetMetadataDefinitions();
            foreach (var key in metadata.Keys)
            {
                var vm = new MetadataViewModel(this) { Name = key, Kind = metadata[key] };
                vm.PropertyChanged += SaveForestSettings;
                ForestMetadata.Add(vm);
            }

            foreach (var s in _dataService.GetCharacters())
            {
                var vm = new CharacterViewModel(this) { Name = s };
                vm.PropertyChanged += SaveForestSettings;
                ForestCharacters.Add(vm);
            }
        }

        private void UpdateNotifications()
        {
            // Clear out all our previous notifications
            _notificationService.RemoveScheduledNotifications();

            // Then schedule a new set for the next 10 days
            if (EnableNotification)
            {
                var title = Resources.DailyNotificationTitle;
                var description = EnableWordTracking ? string.Format(Resources.DailyNotificationDesc, DailyWordObjective) 
                                                     : Resources.DailyNotificationDescNoTrack;

                for (var i = 0; i < 10; i++)
                {
                    _notificationService.ScheduleNotification(title, description, DateTime.Today.AddDays(i), NotificationTime);
                }
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
