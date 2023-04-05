using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Core.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace DialogueForest.Views
{
    public sealed partial class DialogueTreePage : Page
    {
        public DialogueTreeViewModel ViewModel => (DialogueTreeViewModel)DataContext;

        public DialogueTreePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // The DialogueTreeVM can be obtained in multiple different ways.
            if (e.Parameter is DialogueTree t) // Base tree model
            {
                DataContext = DialogueTreeViewModel.Create(t, true);
            }
            else if (e.Parameter is DialogueTreeViewModel existingVm) // Existing VM in case of navigation from the tabpage
            {
                DataContext = existingVm;
            }
            else if (e.Parameter is string s) // Hardcoded special trees mapped to the dataService
            {
                DialogueTree tree;
                var dataService = Ioc.Default.GetRequiredService<ForestDataService>();
                switch (s)
                {
                    case "notes":
                        tree = dataService.GetNotes();
                        break;
                    case "trash":
                        tree = dataService.GetTrash();
                        break;
                    default: throw new Exception("Unknown tag!");
                }

                DataContext = DialogueTreeViewModel.Create(tree);
            }
                
        }

        private void NodeList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count == 1 && e.Items[0] is DialogueNodeViewModel vm)
            {
                // Set the content of the DataPackage to the ID of the node we're dragging
                e.Data.SetText(vm.ID.ToString());

                // We can either Link (when dragging to a Reply Prompt or pinning) or Move (when dragging to another Tree)
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
