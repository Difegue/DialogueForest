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

        public ObservableCollection<DialogueNode> Nodes { get; } = new ObservableCollection<DialogueNode>();
        public bool NoPinnedNodes => Nodes.Count == 0;
        
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

            foreach (var n in _dataService.GetPinnedNodes().Select(id => _dataService.GetNode(id).Item2))
            {
                Nodes.Add(n);
            }
        }

        private void PinNode(DialogueNode node)
        {
            if (!Nodes.Contains(node))
            {
                Nodes.Add(node);
                _dataService.SetPinnedNode(node, true);
            }
        }
        
        [RelayCommand]
        private void UnpinNode(DialogueNode node)
        {
            Nodes.Remove(node);
            _dataService.SetPinnedNode(node, false);
        }

        [RelayCommand]
        private void ShowNode(DialogueNode node)
        {
            var tuple = _dataService.GetNode(node.ID);

            if (tuple != null)
                _navigationService.OpenDialogueNode(tuple.Item1, tuple.Item2);
        }

    }
}
