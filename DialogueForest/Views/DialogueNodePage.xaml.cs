using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.ViewModels;

using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class DialogueNodePage : Page
    {
        public DialogueNodeViewModel ViewModel => (DialogueNodeViewModel)DataContext;

        public DialogueNodePage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<DialogueNodeViewModel>();
        }

    }
}
