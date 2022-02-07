using System;
using System.Collections.ObjectModel;
using System.Linq;

using DialogueForest.Helpers;
using DialogueForest.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using WinUI = Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Interfaces;
using DialogueForest.Services;

namespace DialogueForest.ViewModels
{
    public partial class OpenedNodesViewModel : ObservableObject
    {

        public ObservableCollection<DialogueNodeViewModel> Tabs { get; } = new ObservableCollection<DialogueNodeViewModel>();

        public OpenedNodesViewModel()
        {
            // Link with NavigationService
            var navService = Ioc.Default.GetRequiredService<INavigationService>() as NavigationService;
            navService.NodeTabContainer = this;

            Tabs.CollectionChanged += (s, e) => OnPropertyChanged(nameof(NoTabsOpen));
        }

        public bool NoTabsOpen => Tabs.Count == 0;

        public void OpenNode(DialogueNodeViewModel vm)
        {
            // TODO refine
            Tabs.Add(vm);
        }

        [ICommand]
        private void CloseTab(WinUI.TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is DialogueNodeViewModel item)
            {
                Tabs.Remove(item);
            }
        }
    }
}
