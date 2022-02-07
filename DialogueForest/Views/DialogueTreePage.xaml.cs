using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.ViewModels;
using System;

using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class DialogueTreePage : Page
    {
        public TreeViewModelBase ViewModel => (TreeViewModelBase)DataContext;

        public DialogueTreePage()
        {
            InitializeComponent();
            // TODO
            DataContext = Ioc.Default.GetRequiredService<TreeViewModelBase>();
            ViewModel.Title = "My Tree";
        }
    }
}
