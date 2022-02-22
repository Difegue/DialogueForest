using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        protected IDispatcherService _dispatcherService;
        protected ForestDataService _dataService;


        public ShellViewModelBase(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService, IDialogService dialogService, ForestDataService dataService)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            _dataService = dataService;

            // First View, use that to initialize our DispatcherService
            _dispatcherService.Initialize();

            ((NotificationServiceBase)_notificationService).InAppNotificationRequested += ShowInAppNotification;
            ((NavigationServiceBase)_navigationService).Navigated += OnFrameNavigated;
        }

        [ObservableProperty]
        private bool isBackEnabled;
        [ObservableProperty]
        private string headerText;

        [ICommand]
        private void Save()
        {
            _dataService.SaveForestToFile();
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
