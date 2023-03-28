using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using DialogueForest.ViewModels;
using AppWindowTitleBar = Microsoft.UI.Windowing.AppWindowTitleBar;
using System.Collections.Generic;
using Microsoft.UI;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using WinUIEx;
using Windows.UI;

namespace DialogueForest.Views
{
    public sealed partial class ShellPage : Page
    {
        private AppWindow _appWindow;

        public ShellViewModel ViewModel => (ShellViewModel)DataContext;

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Services.GetService(typeof(ShellViewModel));
            ViewModel.Initialize(shellFrame, navigationView, treeContainer, newTreeItem, notificationHolder, KeyboardAccelerators);

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                // Enable AppWindow
                _appWindow = (Application.Current as App)?.Window.GetAppWindow();
                (Application.Current as App).Window.Activated += Current_Activated;

                var titleBar = _appWindow.TitleBar;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = (Color)Resources["SystemBaseHighColor"];
                titleBar.ButtonInactiveForegroundColor = (Color)Resources["SystemBaseLowColor"];
                titleBar.ExtendsContentIntoTitleBar = true;

                AppTitleBar.Loaded += AppTitleBar_Loaded;
                AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
            }

            tabsView.CustomDragRegion.SizeChanged += Page_SizeChanged;
        }

        private void AppTitleBar_Loaded(object sender, object args)
        {
            UpdateTitleBarLength(navigationView.IsPaneOpen);
        }
        private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTitleBarLength(navigationView.IsPaneOpen);
        }

        // Update the TitleBar based on the inactive/active state of the app
        private void Current_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated)
            {
                AppTitle.Opacity = 0.8;
            }
            else
            {
                AppTitle.Opacity = 1;
            }
        }

        // Update the TitleBar content layout depending on NavigationView DisplayMode
        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            const int topIndent = 16;
            const int expandedIndent = 48;
            int minimalIndent = 104;

            // If the back button is not visible, reduce the TitleBar content indent.
            if (navigationView.IsBackButtonVisible.Equals(NavigationViewBackButtonVisible.Collapsed))
            {
                minimalIndent = 48;
            }

            Thickness currMargin = AppTitleBar.Margin;

            // Set the TitleBar margin dependent on NavigationView display mode
            if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Top)
            {
                AppTitleBar.Margin = new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
            else
            {
                AppTitleBar.Margin = new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
            }
        }

        // Bunch of event listeners to arrange titlebar draggable area
        private void Page_SizeChanged(object sender, SizeChangedEventArgs e) => UpdateTitleBarLength(navigationView.IsPaneOpen);
        private void NavigationView_PaneOpening(NavigationView sender, object args) => UpdateTitleBarLength(true);
        private void NavigationView_PaneClosing(NavigationView sender, NavigationViewPaneClosingEventArgs args)
            => UpdateTitleBarLength(false);

        private void UpdateTitleBarLength(bool isPaneOpen)
        {
            // Awful hardcoded calculations to define the draggable zones
            // CustomDragRegion is calculated by OpenedNodesPage based on the space taken by the opened tabs
            var leftRectWidth = Pane1.ActualWidth + 32 + (isPaneOpen ? navigationView.OpenPaneLength - AppTitleBar.Margin.Left : 0);
            var rightRectWidth = tabsView.CustomDragRegion.ActualWidth;

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var win = (Application.Current as App)?.Window;
                var scaleAdjustment = win.GetDpiForWindow() / 96f;

                List<Windows.Graphics.RectInt32> dragRectsList = new();

                Windows.Graphics.RectInt32 dragRectL;
                dragRectL.X = (int)(AppTitleBar.Margin.Left * scaleAdjustment);
                dragRectL.Y = 0;
                dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectL.Width = (int)(leftRectWidth * scaleAdjustment);
                dragRectsList.Add(dragRectL);

                Windows.Graphics.RectInt32 dragRectR;
                dragRectR.X = (int)((win.Width - rightRectWidth) * scaleAdjustment);
                dragRectR.Y = 0;
                dragRectR.Height = dragRectL.Height;
                dragRectR.Width = (int)(rightRectWidth * scaleAdjustment);
                dragRectsList.Add(dragRectR);

                Windows.Graphics.RectInt32[] dragRects = dragRectsList.ToArray();

                _appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }

        private void NavigationViewItem_DragOver(object sender, DragEventArgs e) => ViewModel.NavigationViewItem_DragOver(sender, e);

        private void NavigationViewItem_Drop(object sender, DragEventArgs e) => ViewModel.NavigationViewItem_Drop(sender, e);
    }
}
