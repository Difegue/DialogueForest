using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;
using DialogueForest.Core.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace DialogueForest.Core.ViewModels
{
    public partial class DialogueTreeViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private OpenedNodesViewModel _openedNodes;
        private INavigationService _navigationService;
        private INotificationService _notificationService;
        private ForestDataService _dataService;

        private DialogueTree _tree;


        public static DialogueTreeViewModel Create(DialogueTree tree, bool canBeRenamed = false)
        {
            var instance = Ioc.Default.GetRequiredService<DialogueTreeViewModel>();
            instance.LoadFromTree(tree);
            instance._canBeRenamed = canBeRenamed;

            return instance;
        }

        private void LoadFromTree(DialogueTree tree)
        {
            _tree = tree;

            foreach (var node in tree.Nodes.Values)
            {
                DialogueNodeViewModel nodeVm;

                // Check if there isn't already a VM for this node to recycle
                if (_openedNodes.Tabs.FirstOrDefault(vm => vm.ID == node.ID) is DialogueNodeViewModel existingVm)
                {
                    nodeVm = existingVm;  
                } 
                else
                {
                    nodeVm = DialogueNodeViewModel.Create(node, this);
                }

                Nodes.Add(nodeVm);
            }
        }

        public List<long> GetIDs() => _tree.Nodes.Keys.ToList();

        public ObservableCollection<DialogueNodeViewModel> Nodes { get; } = new ObservableCollection<DialogueNodeViewModel>();

        public DialogueTreeViewModel(IDialogService dialogService, OpenedNodesViewModel openedNodes, INavigationService navigationService, INotificationService notificationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _openedNodes = openedNodes;
            _notificationService = notificationService;
            _dataService = forestService;

            Nodes.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsNodesEmpty));
            Nodes.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalDialogues));

            WeakReferenceMessenger.Default.Register<DialogueTreeViewModel, NodeMovedMessage>(this, (r, m) => r.UpdateNodes(m));
        }

        private void UpdateNodes(NodeMovedMessage message)
        {
            var node = message.NodeMoved;

            if (_tree == message.SourceTree)
            {
                // Remove Node 
                Nodes.Remove(Nodes.Where(vm => vm.ID == node.ID).FirstOrDefault());
            } 
            
            if (_tree == message.DestinationTree)
            {
                // Add Node
                if (_openedNodes.Tabs.FirstOrDefault(vm => vm.ID == node.ID) is DialogueNodeViewModel existingVm)
                {
                    Nodes.Add(existingVm);
                }
                else
                {
                    Nodes.Add(DialogueNodeViewModel.Create(node, this));
                }
            }
        }

        public bool IsNodesEmpty => Nodes.Count == 0;
        public bool IsTrash => _tree == _dataService.GetTrash();
        public bool IsNotes => _tree == _dataService.GetNotes();
        public bool CannotAddNodes => _tree.CannotAddNodes;

        public string EmptyViewTitle => IsTrash ? Resources.EmptyViewTrashTitle : IsNotes ? Resources.EmptyViewNotesTitle : Resources.EmptyViewTreeTitle;
        public string EmptyViewDesc => IsTrash ? Resources.EmptyViewTrashDesc : IsNotes ? Resources.EmptyViewNotesDesc : Resources.EmptyViewTreeDesc;

        public string Title
        {
            get => _tree.Name;
            set
            {
                SetProperty(_tree.Name, value, _tree, (u, n) => u.Name = n);
                WeakReferenceMessenger.Default.Send(new TreeUpdatedMessage());
            }
        }

        [ObservableProperty]
        private DialogueNodeViewModel _selectedNode;

        [ObservableProperty]
        private int _totalWords;

        [ObservableProperty]
        private bool _canBeRenamed;

        public int TotalDialogues => Nodes.Count;

        [RelayCommand]
        private void ShowNode(DialogueNodeViewModel vm)
        {
            _navigationService.OpenDialogueNode(vm);
        }

        [RelayCommand]
        private void AddNode(DialogueNode node = null)
        {
            if (node == null)
            {
                node = _dataService.CreateNewNode();
            }
            _tree.AddNode(node);
            WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());

            var nodeVm = DialogueNodeViewModel.Create(node, this);
            Nodes.Add(nodeVm);
        }

        internal void MoveNodeToTrash(DialogueNodeViewModel nodeVm, DialogueNode node)
        {
            var trash = _dataService.GetTrash();

            _dataService.MoveNode(node, _tree, trash);
            nodeVm.SetParentVm(_navigationService.ReuseOrCreateTreeVm(trash)); // Hand off parenting to a trash treeVM
            Nodes.Remove(nodeVm);
        }

        [RelayCommand]
        private async Task Delete()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogDeleteTree, Resources.ContentDialogDeleteTreeDesc,
                       Resources.ButtonYesText, Resources.ButtonCancelText))
            {
                _dataService.DeleteTree(_tree);
                _notificationService.ShowInAppNotification(Resources.NotificationTrashed);
                _navigationService.Navigate<DialogueTreeViewModel>("trash");
            }
               
        }

        
    }
}
