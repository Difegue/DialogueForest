using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{
    public abstract partial class ShellViewModelBase : ObservableObject
    {
        protected INavigationService _navigationService;
        protected INotificationService _notificationService;
        protected IDispatcherService _dispatcherService;


        public ShellViewModelBase(INavigationService navigationService, INotificationService notificationService, IDispatcherService dispatcherService)
        {
            _navigationService = navigationService;
            _notificationService = notificationService;
            _dispatcherService = dispatcherService;

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
        private void NewFile()
        {

        }

        [ICommand]
        private void OpenFile()
        {

        }

        [ICommand]
        private void Save()
        {

        }

        [ICommand]
        private void NewTree()
        {

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

            // Use some reflection magic to get the static Header text for this ViewModel
            //var headerMethod = viewModelType.GetMethod(nameof(ViewModelBase.GetHeader), BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            //HeaderText = (string)headerMethod?.Invoke(null, null) ?? "";
        }

    }
}
