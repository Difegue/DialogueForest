using System;

using DialogueForest.ViewModels;

using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class DialogueNodePage : Page
    {
        public DialogueNodeViewModel ViewModel { get; } = new DialogueNodeViewModel();

        public DialogueNodePage()
        {
            InitializeComponent();
        }
    }
}
