using System;

using DialogueForest.ViewModels;

using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class OpenedNodesPage : Page
    {
        public OpenedNodesViewModel ViewModel { get; } = new OpenedNodesViewModel();

        public OpenedNodesPage()
        {
            InitializeComponent();
        }
    }
}
