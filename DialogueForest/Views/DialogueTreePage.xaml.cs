using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Core.ViewModels;
using System;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
                    case "pins":
                        tree = dataService.GetPins();
                        break;
                    default: throw new Exception("Unknown tag!");
                }

                DataContext = DialogueTreeViewModel.Create(tree);
            }
                
        }
    }
}
