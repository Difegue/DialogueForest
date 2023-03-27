using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using DialogueForest.Services;
using DialogueForest.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.WindowManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class OpenedNodesPage : Page
    {
        public OpenedNodesViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<OpenedNodesViewModel>();

        public OpenedNodesPage()
        {
            InitializeComponent();

            // Link with NavigationService
            var navService = Ioc.Default.GetRequiredService<INavigationService>() as NavigationService;
            navService.NodeTabContainer = ViewModel;

            SetupWindow(null);
        }

        private void TabView_TabCloseRequested(Microsoft.UI.Xaml.Controls.TabView sender, Microsoft.UI.Xaml.Controls.TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is DialogueNodeViewModel item)
            {
                ViewModel.RemoveTabCommand.Execute(item);
            }
        }

        void SetupWindow(AppWindow window)
        {
            return;
            
            if (window == null)
            {
                // Hook onto titlebar
                var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
                coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            }
            else
            {
                // Secondary AppWindows --- keep track of the window
                // RootAppWindow = window;

                // Extend into the titlebar
                window.TitleBar.ExtendsContentIntoTitleBar = true;
                window.TitleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                window.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;

                // Due to a bug in AppWindow, we cannot follow the same pattern as CoreWindow when setting the min width.
                // Instead, set a hardcoded number. 
                CustomDragRegion.MinWidth = 188;

                //window.Frame.DragRegionVisuals.Add(CustomDragRegion);
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // To ensure that the tabs in the titlebar are not occluded by shell
            // content, we must ensure that we account for left and right overlays.
            // In LTR layouts, the right inset includes the caption buttons and the
            // drag region, which is flipped in RTL. 

            // The SystemOverlayLeftInset and SystemOverlayRightInset values are
            // in terms of physical left and right. Therefore, we need to flip
            // then when our flow direction is RTL.
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayRightInset;
            }
            else
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayLeftInset;
            }

            // Ensure that the height of the custom regions are the same as the titlebar.
            CustomDragRegion.Height = sender.Height;
        }

    }
}
