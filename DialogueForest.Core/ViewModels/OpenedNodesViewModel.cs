using System;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DialogueForest.Core.ViewModels
{
    public partial class OpenedNodesViewModel : ObservableObject
    {

        public ObservableCollection<DialogueNodeViewModel> Tabs { get; } = new ObservableCollection<DialogueNodeViewModel>();

        [ObservableProperty]
        private DialogueNodeViewModel _selectedItem;

        public OpenedNodesViewModel()
        {
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
                else
                {
                    vm = Tabs.First(t => t.ID == vm.ID);
                }
            }

            SelectedItem = vm;
        }

        [ICommand]
        private void RemoveTab(DialogueNodeViewModel item) => Tabs.Remove(item);                

    }
}
