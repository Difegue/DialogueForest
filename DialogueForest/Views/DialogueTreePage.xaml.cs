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
            DialogueTree tree = null;

            if (e.Parameter is DialogueTree t)
            {
                tree = t;
            }   
            else if (e.Parameter is string s)
            {
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
            }

            if (tree != null)
                DataContext = DialogueTreeViewModel.Create(tree);
        }
    }
}
