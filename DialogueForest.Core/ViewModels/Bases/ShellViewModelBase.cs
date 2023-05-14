using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{
    public abstract partial class ShellViewModelBase : ObservableObject
    {
        protected INavigationService _navigationService;
        protected INotificationService _notificationService;
        protected IDialogService _dialogService;
        protected IInteropService _interopService;
        protected IDispatcherService _dispatcherService;
        protected ForestDataService _dataService;
        protected WordCountingService _wordService;
        
        protected PinnedNodesViewModel _pinnedNodesVm;

        public ShellViewModelBase(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, 
            IDialogService dialogService, IInteropService interopService, ForestDataService dataService, WordCountingService wordService, PinnedNodesViewModel pinnedVm)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            _dataService = dataService;
            _wordService = wordService;
            _interopService = interopService;

            _pinnedNodesVm = pinnedVm;

            // HACK: init settings here just to make sure notifications are rehydrated on every app launch
            Ioc.Default.GetRequiredService<SettingsViewModel>().EnsureInstanceInitialized();

            // Listeners to set unsaved/saved document status
            WeakReferenceMessenger.Default.Register<ShellViewModelBase, TreeUpdatedMessage>(this, (r, m) =>
            {
                r.SetIsDirty(m.SetDirty);
                r.UpdateTreeList();
            });
            WeakReferenceMessenger.Default.Register<ShellViewModelBase, SavedFileMessage>(this, (r, m) => r.SetIsDirty(false));
            WeakReferenceMessenger.Default.Register<ShellViewModelBase, NodeMovedMessage>(this, (r, m) => r.SetIsDirty(true));
            WeakReferenceMessenger.Default.Register<ShellViewModelBase, ForestSettingsChangedMessage>(this, (r, m) => r.SetIsDirty(true));
            WeakReferenceMessenger.Default.Register<ShellViewModelBase, UnsavedModificationsMessage>(this, (r, m) => r.SetIsDirty(true));

            WeakReferenceMessenger.Default.Register<ShellViewModelBase, SettingsChangedMessage>(this, (r, m) => UpdateWordTrackingInfo());

            UpdateTitleBar();
            UpdateWordTrackingInfo();

            ((NotificationServiceBase)_notificationService).InAppNotificationRequested += ShowInAppNotification;
            ((NavigationServiceBase)_navigationService).Navigated += OnFrameNavigated;
            _wordService.WordCountUpdated += (s, e) => UpdateWordTrackingInfo();
        }

        public void ShutdownInitiated()
        {
            // Always autosave on shutdown
            _dataService.SaveForestToStorage();
        }

        private void SetIsDirty(bool isDirty)
        {
            _dataService.SetForestDirty(isDirty);
            _dispatcherService.ExecuteOnUIThreadAsync(() => {
                OnPropertyChanged(nameof(HasUnsavedChanges));
                UpdateTitleBar();
            });
        }

        private void UpdateTitleBar()
        {
            var file = _dataService.LastSavedFile;

            if (file != null)
                _interopService.UpdateAppTitle(file.Name + file.Extension + " - " + Resources.AppDisplayName);
            else
                _interopService.UpdateAppTitle(Resources.AppDisplayName);

            DisplayName = (file == null) ? Resources.NavigationNew : file.Name + file.Extension;
            TitleBarText = Resources.AppDisplayName + " - " + DisplayName + (HasUnsavedChanges ? "*" : "");
        }

        private void UpdateWordTrackingInfo()
        {
            _dispatcherService.ExecuteOnUIThreadAsync(() => OnPropertyChanged(nameof(WordTrackingEnabled)));

            DailyWordCount = $"{_wordService.CurrentWordCount} / {_wordService.CurrentWordObjective}";
            DailyWordCountPercentage = _wordService.CurrentWordCount / (float)_wordService.CurrentWordObjective * 100f;
            DailyStreak = String.Format(Resources.DailyObjectiveStreakTitle, _wordService.CurrentStreak);
            DailyObjectiveComplete = _wordService.CurrentWordCount >= _wordService.CurrentWordObjective;
        }
        
        [ObservableProperty]
        private bool _isBackEnabled;
        [ObservableProperty]
        private string _titleBarText;
        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private float _dailyWordCountPercentage;
        [ObservableProperty]
        private string _dailyWordCount;
        [ObservableProperty]
        private string _dailyStreak;
        [ObservableProperty]
        private bool _dailyObjectiveComplete;

        public bool HasUnsavedChanges => _dataService.CurrentForestHasUnsavedChanges;
        public bool WordTrackingEnabled => _wordService.IsTrackingEnabled;

        [RelayCommand]
        private async Task Open()
        {
            await _dataService.LoadForestFromFileAsync();
        }

        [RelayCommand]
        private async Task Save()
        {
            await _dataService.SaveForestToStorageAsync();
            await _dataService.SaveForestToFileAsync();
        }

        [RelayCommand]
        private async Task NewTree()
        {
            var name = await _dialogService.ShowTreeNameDialogAsync();

            if (name != null)
            {
                var tree = _dataService.CreateNewTree(name);
                UpdateTreeList();
                _navigationService.Navigate<DialogueTreeViewModel>(tree);
            }

        }

        [RelayCommand]
        protected abstract void Loaded();
        [RelayCommand]
        protected abstract void ItemInvoked(object item);

        protected abstract void ShowInAppNotification(object sender, InAppNotificationRequestedEventArgs e);
        protected abstract void UpdateTreeList();

        private void OnFrameNavigated(object sender, CoreNavigationEventArgs e)
        {
            IsBackEnabled = _navigationService.CanGoBack;

            var viewModelType = e.NavigationTarget;
            if (viewModelType == null) return;
        }

    }
}
