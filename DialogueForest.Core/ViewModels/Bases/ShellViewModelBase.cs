using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
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


        public ShellViewModelBase(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IDialogService dialogService, IInteropService interopService, ForestDataService dataService)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            _dataService = dataService;
            _interopService = interopService;

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

            UpdateTitleBar();

            ((NotificationServiceBase)_notificationService).InAppNotificationRequested += ShowInAppNotification;
            ((NavigationServiceBase)_navigationService).Navigated += OnFrameNavigated;

            // TODO
            DailyStreak = String.Format(Resources.DailyObjectiveStreakTitle, 3);
        }

        public void ShutdownInitiated()
        {
            _dataService.SaveForestToStorage();
        }

        private void SetIsDirty(bool isDirty)
        {
            HasUnsavedChanges = isDirty;
            UpdateTitleBar();
        }

        private void UpdateTitleBar()
        {
            var file = _dataService.LastSavedFile;

            if (file != null)
                _interopService.UpdateAppTitle(file.Name + file.Extension);

            DisplayName = (file == null) ? Resources.NavigationNew : file.Name + file.Extension;
            TitleBarText = Resources.AppDisplayName + " - " + DisplayName + (HasUnsavedChanges ? "*" : "");
        }

        [ObservableProperty]
        private bool _hasUnsavedChanges;
        [ObservableProperty]
        private bool _isBackEnabled;
        [ObservableProperty]
        private string _titleBarText;
        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private int _dailyWordCountPercentage;
        [ObservableProperty]
        private string _dailyWordCount;
        [ObservableProperty]
        private string _dailyStreak;
        [ObservableProperty]
        private bool _dailyObjectiveComplete;

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
