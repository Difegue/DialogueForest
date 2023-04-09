using System;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{

    public partial class PinnedNodesViewModel : ObservableObject
    {
        private readonly ForestDataService _dataService;
        private readonly INavigationService _navigationService;

        public ObservableCollection<DialogueNodeViewModel> Nodes { get; } = new();
        public bool NoPinnedNodes => Nodes.Count == 0;

        // Dumb property that only serves to trigger opening nodes selected in datagrid
        private DialogueNodeViewModel _selectedPin;
        public DialogueNodeViewModel SelectedPin
        {
            get => _selectedPin;
            set { if (value != null) _navigationService.OpenDialogueNode(value); }
        }

        public PinnedNodesViewModel(INavigationService navService, ForestDataService dataService)
        {
            _navigationService = navService;
            _dataService = dataService;
            
            Nodes = new();
            Nodes.CollectionChanged += (s, e) => OnPropertyChanged(nameof(NoPinnedNodes));

            WeakReferenceMessenger.Default.Register<PinnedNodesViewModel, TreeUpdatedMessage>(this, (r,m) => r.RefreshPinnedNodes());
            WeakReferenceMessenger.Default.Register<PinnedNodesViewModel, UnsavedModificationsMessage>(this, (r, m) => r.OnPropertyChanged(nameof(Nodes)));
            WeakReferenceMessenger.Default.Register<PinnedNodesViewModel, AskToPinNodeMessage>(this, (r, m) => 
            { 
                if (m.pinStatus) 
                    r.PinNode(m.node); 
                else 
                    r.UnpinNode(m.node); 
            });
        }

        private void RefreshPinnedNodes()
        {
            Nodes.Clear();

            foreach (var tuple in _dataService.GetPinnedNodes().Select(id => _dataService.GetNode(id)))
            {
                var node = tuple.Item2;
                var treeVm = _navigationService.ReuseOrCreateTreeVm(tuple.Item1);

                Nodes.Add(DialogueNodeViewModel.Create(node, treeVm));
            }
        }

        private void PinNode(DialogueNode node)
        {
            if (!Nodes.Select(vm => vm.ID).Contains(node.ID))
            {
                var tuple = _dataService.GetNode(node.ID);
                var treeVm = _navigationService.ReuseOrCreateTreeVm(tuple.Item1);

                Nodes.Add(DialogueNodeViewModel.Create(node, treeVm));
                _dataService.SetPinnedNode(node, true);
            }
        }
        
        [RelayCommand]
        private void UnpinNode(DialogueNode node)
        {
            Nodes.Remove(Nodes.Where(vm => vm.ID == node.ID).FirstOrDefault());
            _dataService.SetPinnedNode(node, false);
        }

        [RelayCommand]
        private void ShowNode(DialogueNode node)
        {
            var tuple = _dataService.GetNode(node.ID);

            if (tuple != null)
                _navigationService.OpenDialogueNode(tuple.Item1, tuple.Item2);
        }

        public void SortPins(string order, string tag)
        {
            // Sort the Nodes collection by tag (ID, Name or Tree) and order (Asc or Desc)
            var sortedList = order == "Asc" ? Nodes.OrderBy(n => tag == "ID" ? n.ID.ToString() : tag == "Title" ? n.NodeTitle : n.TreeTitle).ToList() :
                                    Nodes.OrderByDescending(n => tag == "ID" ? n.ID.ToString() : tag == "Title" ? n.NodeTitle : n.TreeTitle).ToList();

            Nodes.Clear();
            foreach (var node in sortedList)
                Nodes.Add(node);
        }
    }

}
