using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Core.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using DialogueForest.ViewModels;
using Microsoft.UI.Xaml;

namespace DialogueForest.Views
{
    public sealed partial class PinsPage : Page
    {
        public PinnedNodesViewModel ViewModel => (PinnedNodesViewModel)DataContext;

        public PinsPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Services.GetService(typeof(PinnedNodesViewModel));
        }

        private void NodeList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count == 1 && e.Items[0] is DialogueNodeViewModel vm)
            {
                // Set the content of the DataPackage to the ID of the node we're dragging
                e.Data.SetText(vm.ID.ToString());

                // We can either Link (when dragging to a Reply Prompt) or Move (when dragging to another Tree)
                e.Data.RequestedOperation = DataPackageOperation.Link | DataPackageOperation.Move;
            }
        }

        private void AddLinkFlyoutHandler(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var menu = sender as MenuFlyoutSubItem;
            menu.AddHandler(PointerEnteredEvent, new PointerEventHandler((sender, e) =>
                            Helpers.UWPHelpers.LoadLinkedNodesIntoMenuFlyout(menu, menu.DataContext as DialogueNodeViewModel)), true);
        }
            
    }
}
