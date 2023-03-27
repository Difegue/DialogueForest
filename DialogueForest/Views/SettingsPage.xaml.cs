using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.ViewModels;

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DialogueForest.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

        public SettingsPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<SettingsViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.EnsureInstanceInitialized();
        }
    }
}
