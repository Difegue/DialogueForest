using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.System;
using DialogueForest.Core.ViewModels;
using DialogueForest.Core.Interfaces;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using WinUI = Microsoft.UI.Xaml.Controls;
using DialogueForest.Services;
using Windows.Foundation;
using DialogueForest.Helpers;
using System.Windows.Input;
using DialogueForest.Core.Services;
using Windows.UI.Xaml.Media;
using DialogueForest.Core.Models;

namespace DialogueForest.ViewModels
{
    public class ShellViewModel : ShellViewModelBase
    {
        private IList<KeyboardAccelerator> _keyboardAccelerators;
        private KeyboardAccelerator _altLeftKeyboardAccelerator;
        private KeyboardAccelerator _backKeyboardAccelerator;

        private WinUI.NavigationView _navigationView;
        private WinUI.NavigationViewItem _treeContainer;
        private WinUI.NavigationViewItem _newTreeItem;
        private InAppNotification _notificationHolder;


        public ShellViewModel(INavigationService navigationService, INotificationService notificationService, IDialogService dialogService, IDispatcherService dispatcherService, ForestDataService dataService) :
            base(navigationService, notificationService, dispatcherService, dialogService, dataService)
        {
        }

        public void Initialize(Frame frame, WinUI.NavigationView navigationView, WinUI.NavigationViewItem treeContainer, WinUI.NavigationViewItem newTreeItem, InAppNotification notificationHolder, IList<KeyboardAccelerator> keyboardAccelerators)
        {
            _navigationView = navigationView;
            _treeContainer = treeContainer;
            _newTreeItem = newTreeItem;
            _notificationHolder = notificationHolder;
            _keyboardAccelerators = keyboardAccelerators;

            var concreteNavService = (NavigationService)_navigationService;
            concreteNavService.Frame = frame;
            concreteNavService.Navigated += UpdateNavigationViewSelection; 

            _navigationView.BackRequested += OnBackRequested;

            _altLeftKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.Left, GoBack, VirtualKeyModifiers.Menu);
            _backKeyboardAccelerator = BuildKeyboardAccelerator(VirtualKey.GoBack, GoBack);
        }

        private void UpdateNavigationViewSelection(object sender, CoreNavigationEventArgs e)
        {
            if (e.NavigationTarget == typeof(SettingsViewModel))
            {
                _navigationView.SelectedItem = _navigationView.SettingsItem as WinUI.NavigationViewItem;
                return;
            }

            if (e.Parameter is DialogueTree)
            {
                _navigationView.SelectedItem = _treeContainer.MenuItems
                           .OfType<WinUI.NavigationViewItem>()
                           .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e));
            }
            else
            {
                _navigationView.SelectedItem = _navigationView.MenuItems
                           .OfType<WinUI.NavigationViewItem>()
                           .FirstOrDefault(menuItem => IsMenuItemForPageType(menuItem, e));
            }   
        }

        private bool IsMenuItemForPageType(WinUI.NavigationViewItem menuItem, CoreNavigationEventArgs e)
        {
            var pageType = menuItem.GetValue(NavHelper.NavigateToProperty) as Type;

            bool isParameterMatching;
            if (e.Parameter is string s)
                isParameterMatching = s == (string)menuItem.Tag;
            else
                isParameterMatching = e.Parameter == menuItem.Tag;

            return pageType == e.NavigationTarget && isParameterMatching;
        }

        protected override void ShowInAppNotification(object sender, InAppNotificationRequestedEventArgs e)
        {
            _dispatcherService.ExecuteOnUIThreadAsync(() => _notificationHolder.Show(e.NotificationText, e.NotificationTime));
        }

        protected override void UpdateTreeList()
        {
            // Update the navigationview by hand - It ain't clean but partial databinding would be an even bigger mess...
            var trees = _dataService.GetDialogueTrees();

            // Remove all menuitems in the "Trees" menu
            _treeContainer.MenuItems.Clear();

            foreach (var tree in trees)
            {
                var navigationViewItem = new WinUI.NavigationViewItem();
                navigationViewItem.Icon = new FontIcon { Glyph = "\uEC0A" };
                navigationViewItem.Content = tree.Name;
                navigationViewItem.Tag = tree;
                NavHelper.SetNavigateTo(navigationViewItem, typeof(DialogueTreeViewModel));
                _treeContainer.MenuItems.Add(navigationViewItem);
            }

            _treeContainer.MenuItems.Add(_newTreeItem);
        }

        protected override void Loaded()
        {
            // Keyboard accelerators are added here to avoid showing 'Alt + left' tooltip on the page.
            // More info on tracking issue https://github.com/Microsoft/microsoft-ui-xaml/issues/8
            _keyboardAccelerators.Add(_altLeftKeyboardAccelerator);
            _keyboardAccelerators.Add(_backKeyboardAccelerator);
        }

        protected override void ItemInvoked(object args)
        {
            var navArgs = (WinUI.NavigationViewItemInvokedEventArgs)args;

            if (navArgs.IsSettingsInvoked)
            {
                _navigationService.Navigate<SettingsViewModel>();
                return;
            }

            var item = navArgs.InvokedItemContainer;

            if (item == null)
                return;

            // Try executing the command first if there's one attached to this navItem
            var command = item.GetValue(NavHelper.CommandProperty) as ICommand;
            if (command != null)
            {
                command.Execute(item.GetValue(NavHelper.CommandParameterProperty));
                return;
            }

            var pageType = item.GetValue(NavHelper.NavigateToProperty) as Type;
            _navigationService.Navigate(pageType, item.Tag);
        }

        private void OnBackRequested(WinUI.NavigationView sender, WinUI.NavigationViewBackRequestedEventArgs args)
        {
            _navigationService.GoBack();
        }

        private KeyboardAccelerator BuildKeyboardAccelerator(VirtualKey key, TypedEventHandler<KeyboardAccelerator, KeyboardAcceleratorInvokedEventArgs> onInvoked, VirtualKeyModifiers? modifiers = null)
        {
            var keyboardAccelerator = new KeyboardAccelerator() { Key = key };
            if (modifiers.HasValue)
            {
                keyboardAccelerator.Modifiers = modifiers.Value;
            }

            keyboardAccelerator.Invoked += onInvoked;
            return keyboardAccelerator;
        }

        private void GoBack(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var result = _navigationService.GoBack();
            args.Handled = result;
        }
    }
}
