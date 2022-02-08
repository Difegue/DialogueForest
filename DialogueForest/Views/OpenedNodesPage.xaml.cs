using System;

using DialogueForest.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class OpenedNodesPage : Page
    {
        public OpenedNodesViewModel ViewModel { get; } = new OpenedNodesViewModel();

        public OpenedNodesPage()
        {
            InitializeComponent();

            SetupWindow(null);
        }

        void SetupWindow(AppWindow window)
        {
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
                window.TitleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
                window.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.Transparent;

                // Due to a bug in AppWindow, we cannot follow the same pattern as CoreWindow when setting the min width.
                // Instead, set a hardcoded number. 
                CustomDragRegion.MinWidth = 188;

                window.Frame.DragRegionVisuals.Add(CustomDragRegion);
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
