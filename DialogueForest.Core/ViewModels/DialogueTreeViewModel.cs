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
using YA;

namespace DialogueForest.Core.ViewModels
{
    public partial class DialogueTreeViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private OpenedNodesViewModel _openedNodes;
        private INavigationService _navigationService;
        private INotificationService _notificationService;
        private IApplicationStorageService _storageService;
        private ForestDataService _dataService;

        private DialogueTree _tree;
        private readonly Dictionary<string, MarkerViewModel> _markerVmCache = new();
        private ITreeListItem _lastRemovedItem;

        public static DialogueTreeViewModel Create(DialogueTree tree, bool canBeRenamed = false)
        {
            var instance = Ioc.Default.GetRequiredService<DialogueTreeViewModel>();
            instance.LoadFromTree(tree);
            instance.CanBeRenamed = canBeRenamed;

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

        public FilterableObservableCollection<DialogueNodeViewModel> Nodes { get; } = new();
        public IEnumerable<DialogueNodeViewModel> ParentNodes => Nodes.Where(n => n.GetNodesLinkingToUs().Count == 0);

        // Merged, ordered display collection (nodes + markers) that drives the List pane.
        public ObservableCollection<ITreeListItem> TreeItems { get; } = new();

        public DialogueTreeViewModel(IDialogService dialogService, OpenedNodesViewModel openedNodes, INavigationService navigationService, INotificationService notificationService,
            IApplicationStorageService storageService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _openedNodes = openedNodes;
            _notificationService = notificationService;
            _storageService = storageService;
            _dataService = forestService;

            _showTreeView = _storageService.GetValue(nameof(ShowTreeView), false);

            Nodes.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsNodesEmpty));
                OnPropertyChanged(nameof(TotalDialogues));
                OnPropertyChanged(nameof(TotalWords));
                OnPropertyChanged(nameof(ParentNodes));

                RebuildTreeItems();
            };

            // GridView drag-reorder operates on the merged TreeItems collection (nodes + markers),
            // implemented by the control as Remove + Add of the same item.
            TreeItems.CollectionChanged += (s, e) =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove
                && e.OldItems?[0] is ITreeListItem removed)
                {
                    _lastRemovedItem = removed;
                }

                // Support for moving nodes and markers, gridview implements it as remove + add
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add
                && e.NewItems?[0] is ITreeListItem added && ReferenceEquals(added, _lastRemovedItem))
                {
                    var displayIndex = e.NewStartingIndex;

                    if (added is DialogueNodeViewModel nodeVm)
                    {
                        // Translate the display index into a true node index (count nodes before the drop).
                        int nodeIndex = 0;
                        for (int i = 0; i < displayIndex && i < TreeItems.Count; i++)
                            if (TreeItems[i] is DialogueNodeViewModel) nodeIndex++;
                        _dataService.MoveNodeInTree(nodeVm.ID, _tree, nodeIndex);
                    }
                    else if (added is MarkerViewModel markerVm)
                    {
                        // Anchor the marker before the next node after its new slot (0 => trailing).
                        long newAnchor = 0;
                        for (int i = displayIndex + 1; i < TreeItems.Count; i++)
                            if (TreeItems[i] is DialogueNodeViewModel n) { newAnchor = n.ID; break; }
                        _dataService.MoveMarker(markerVm.Model, newAnchor);
                    }

                    _lastRemovedItem = null;
                }
            };

            WeakReferenceMessenger.Default.Register<DialogueTreeViewModel, UnsavedModificationsMessage>(this, (r, m) => r.OnPropertyChanged(nameof(TotalWords)));
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

        // Dumb property that only serves to trigger opening nodes selected in treeview
        private DialogueNodeViewModel _selectedNode;
        public DialogueNodeViewModel SelectedNode

        {
            get => _selectedNode;
            set { if (value != null) _navigationService.OpenDialogueNode(value); }
        }

        [ObservableProperty]
        private bool _canBeRenamed;

        [ObservableProperty]
        private bool _showTreeView;

        partial void OnShowTreeViewChanged(bool value) => _storageService.SetValue(nameof(ShowTreeView), value);

        [ObservableProperty]
        private string _searchString;

        partial void OnSearchStringChanged(string value)
        {
            Nodes.Filter = (node) => (node.ID.ToString() + node.NodeTitle?.ToLowerInvariant() + node.TextSummary?.ToLowerInvariant())
                                        .Contains(value.ToLowerInvariant());
        }

        public int TotalDialogues => Nodes.Count;

        public int TotalWords => Nodes.Sum(n => n.WordCount);

        [RelayCommand]
        private void ShowNode(ITreeListItem item)
        {
            // Markers are click-inert in the list; only real nodes open.
            if (item is DialogueNodeViewModel vm)
                _navigationService.OpenDialogueNode(vm);
        }

        // Rebuild the merged (nodes + markers) display collection in persisted node order.
        internal void RebuildTreeItems()
        {
            TreeItems.Clear();

            var markersByAnchor = _tree.Markers
                .Where(m => m.BeforeNodeId != 0)
                .ToLookup(m => m.BeforeNodeId);

            // Only node VMs currently present in Nodes are visible (respects the active filter).
            var visible = new Dictionary<long, DialogueNodeViewModel>();
            foreach (var vm in Nodes)
                visible[vm.ID] = vm;

            foreach (var id in _tree.Nodes.Keys) 
            {
                foreach (var marker in markersByAnchor[id])
                    TreeItems.Add(GetOrCreateMarkerVm(marker));

                if (visible.TryGetValue(id, out var nodeVm))
                    TreeItems.Add(nodeVm);
            }

            // Trailing markers (BeforeNodeId == 0) and orphaned markers render at the end.
            foreach (var marker in _tree.Markers.Where(m => m.BeforeNodeId == 0 || !_tree.Nodes.ContainsKey(m.BeforeNodeId)))
                TreeItems.Add(GetOrCreateMarkerVm(marker));
        }

        private MarkerViewModel GetOrCreateMarkerVm(DialogueMarker marker)
        {
            if (!_markerVmCache.TryGetValue(marker.Id, out var vm))
            {
                vm = new MarkerViewModel(marker, this);
                _markerVmCache[marker.Id] = vm;
            }
            return vm;
        }

        [RelayCommand]
        private void AddMarker()
        {
            // New marker sits before the first node (top of the list); user renames it inline.
            var anchor = _tree.Nodes.Keys.FirstOrDefault(); // 0 (trailing) when the tree is empty
            var marker = new DialogueMarker
            {
                Id = System.Guid.NewGuid().ToString(),
                Label = Resources.MarkerDefaultLabel,
                BeforeNodeId = anchor
            };
            _dataService.AddMarker(_tree, marker);
            RebuildTreeItems();
        }

        internal void DeleteMarker(MarkerViewModel vm)
        {
            _dataService.RemoveMarker(_tree, vm.Model);
            _markerVmCache.Remove(vm.Model.Id);
            RebuildTreeItems();
        }

        // Move a marker by re-anchoring it. 
        internal void MoveMarkerRelative(MarkerViewModel vm, int dir)
        {
            var order = _tree.Nodes.Keys.ToList();
            var marker = vm.Model;

            int anchorIdx = marker.BeforeNodeId != 0 ? order.IndexOf(marker.BeforeNodeId) : order.Count;
            if (anchorIdx < 0) anchorIdx = order.Count; // orphaned => treat as trailing

            int newIdx = anchorIdx + dir;
            if (newIdx < 0) newIdx = 0;
            if (newIdx > order.Count) newIdx = order.Count;

            long newAnchor = newIdx < order.Count ? order[newIdx] : 0;
            _dataService.MoveMarker(marker, newAnchor);
            RebuildTreeItems();
        }

        [RelayCommand]
        private void AddNode(DialogueNode node = null)
        {
            var newNode = AddAndReturnNode(node);
            _navigationService.OpenDialogueNode(newNode);
        }

        internal DialogueNodeViewModel AddAndReturnNode(DialogueNode node = null)
        {
            if (node == null)
            {
                node = _dataService.CreateNewNode();
            }
            _tree.AddNode(node);
            WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());

            var nodeVm = DialogueNodeViewModel.Create(node, this);
            Nodes.Add(nodeVm);

            return nodeVm;
        }

        internal void MoveNodeToTrash(DialogueNodeViewModel nodeVm, DialogueNode node)
        {
            var trash = _dataService.GetTrash();

            _dataService.MoveNode(node, _tree, trash);
            nodeVm.SetParentVm(_navigationService.ReuseOrCreateTreeVm(trash)); // Hand off parenting to a trash treeVM
            Nodes.Remove(nodeVm);
        }

        internal void DeleteNode(DialogueNodeViewModel nodeVm, DialogueNode node)
        {
            _dataService.DeleteNode(node);
            Nodes.Remove(nodeVm); // This is the trash treeVM, so we can just remove the node
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

        [RelayCommand]
        private async Task EmptyTrash()
        {

            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogEmptyTrash, Resources.ContentDialogEmptyTrashDesc,
                       Resources.ButtonYesText, Resources.ButtonCancelText))
            {
                foreach (var nodeVm in Nodes.ToList())
                {
                    // Slightly roundabout way but easier since nodeVm will give us the node model
                    nodeVm.DeleteNode();
                }

                _notificationService.ShowInAppNotification(Resources.NotificationTrashEmptied);
            }

        }
    }
}
