using System;

using DialogueForest.ViewModels;

using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    // For more info about the TabView Control see
    // https://docs.microsoft.com/uwp/api/microsoft.ui.xaml.controls.tabview?view=winui-2.2
    // For other samples, get the XAML Controls Gallery app http://aka.ms/XamlControlsGallery
    public sealed partial class OpenedNodesPage : Page
    {
        public OpenedNodesViewModel ViewModel { get; } = new OpenedNodesViewModel();

        public OpenedNodesPage()
        {
            InitializeComponent();
        }
    }
}
