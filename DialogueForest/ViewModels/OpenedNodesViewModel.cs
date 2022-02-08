using System;
using System.Collections.ObjectModel;
using System.Linq;

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

        [ObservableProperty]
        private int _selectedTabIndex;

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
            // Check if this node isn't already open
            if (!Tabs.Contains(vm))
            {
                // Make 'really' sure
                if (!Tabs.Select(t => t.ID).Contains(vm.ID))
                {
                    // TODO: Remove the current SelectedVM, unless it has unsaved changes?
                    Tabs.Add(vm);
                }
            }

            SelectedTabIndex = Tabs.IndexOf(vm);
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
