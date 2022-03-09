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

            // First View, use that to initialize our DispatcherService
            _dispatcherService.Initialize();

            WeakReferenceMessenger.Default.Register<ShellViewModelBase, SavedFileMessage>(this, (r, m) => r.Receive(m));
            _titleBarText = _dataService.LastSavedFile != null ? UpdateTitleBar() : Resources.AppDisplayName;

            ((NotificationServiceBase)_notificationService).InAppNotificationRequested += ShowInAppNotification;
            ((NavigationServiceBase)_navigationService).Navigated += OnFrameNavigated;
        }

        private string UpdateTitleBar()
        {
            var file = _dataService.LastSavedFile;
            _interopService.UpdateAppTitle(file.Name + file.Extension);
            return Resources.AppDisplayName + " - " + file.Name + file.Extension;
        }

        private void Receive(SavedFileMessage m) => _titleBarText = UpdateTitleBar();

        [ObservableProperty]
        private bool _isBackEnabled;
        [ObservableProperty]
        private string _titleBarText;

        [ICommand]
        private async Task Open()
        {
            await _dataService.LoadForestFromFileAsync();
        }

        [ICommand]
        private async Task Save()
        {
            await _dataService.SaveForestToStorageAsync();
            await _dataService.SaveForestToFileAsync();
        }

        [ICommand]
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

        [ICommand]
        protected abstract void Loaded();
        [ICommand]
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
