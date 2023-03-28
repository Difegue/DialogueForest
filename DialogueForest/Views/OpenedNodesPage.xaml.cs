using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using DialogueForest.Services;
using DialogueForest.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.UI.WindowManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class OpenedNodesPage : Page
    {
        public OpenedNodesViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<OpenedNodesViewModel>();

        public OpenedNodesPage()
        {
            InitializeComponent();

            // Link with NavigationService
            var navService = Ioc.Default.GetRequiredService<INavigationService>() as NavigationService;
            navService.NodeTabContainer = ViewModel;
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is DialogueNodeViewModel item)
            {
                ViewModel.RemoveTabCommand.Execute(item);
            }
        }

    }
}
