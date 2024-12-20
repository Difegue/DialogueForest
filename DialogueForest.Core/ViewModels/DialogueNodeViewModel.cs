﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{

    public partial class DialogueNodeViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private INavigationService _navigationService;
        private INotificationService _notificationService;
        private ForestDataService _dataService;

        private DialogueTreeViewModel _parentVm;
        private DialogueNode _node;

        internal static DialogueNodeViewModel Create(DialogueNode node, DialogueTreeViewModel parentVm)
        {
            if (node == null) return null;

            var instance = Ioc.Default.GetRequiredService<DialogueNodeViewModel>();
            instance.SetParentVm(parentVm);
            instance.LoadFromNode(node);

            return instance;
        }

        /// <summary>
        ///  Get all the IDs of the Tree this node belongs to.
        /// </summary>
        /// <returns></returns>
        internal List<long> GetIDs()
        {
            var allIDs = _parentVm.GetIDs();
            allIDs.Remove(ID); // Remove ourselves
            return allIDs;
        }

        internal long CreateNewDialogue()
        {
            DialogueNodeViewModel newVm = _parentVm.AddAndReturnNode();
            return newVm.ID;
        }

        private void LoadFromNode(DialogueNode node)
        {
            _node = node;

            UpdateMetadata();

            foreach (var prompt in node.Prompts)
                AddPrompt(prompt);

            foreach (var dialogue in node.DialogueLines)
                AddDialog(dialogue);

            OnPropertyChanged(nameof(TextSummary));
        }

        private void UpdateMetadata()
        {
            MetaValues.Clear();
            foreach (var kvp in _dataService.GetMetadataDefinitions())
            {
                // Check if the node already has metadata for this key
                if (_node.Metadata.FirstOrDefault(m => m.Key == kvp.Key) is DialogueMetadataValue metadata)
                {
                    // In case the kind changed, update it
                    metadata.Kind = kvp.Value;
                    AddMetadata(metadata);
                }
                else
                {
                    // Otherwise, create a new value holder
                    var value = new DialogueMetadataValue { Key = kvp.Key, Kind = kvp.Value };
                    _node.Metadata.Add(value);
                    AddMetadata(value);
                }
            }
        }

        public DialogueNodeViewModel(IDialogService dialogService, INavigationService navigationService, INotificationService notificationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _navigationService = navigationService;
            _dataService = forestService;

            WeakReferenceMessenger.Default.Register<DialogueNodeViewModel, ForestSettingsChangedMessage>(this, (r, m) => r.UpdateMetadata());
            WeakReferenceMessenger.Default.Register<DialogueNodeViewModel, TreeUpdatedMessage>(this, (r, m) => 
                r.OnPropertyChanged(nameof(TreeTitle)));

            WeakReferenceMessenger.Default.Register<DialogueNodeViewModel, NodeMovedMessage>(this, (r, m) => r.UpdateParent(m));
            WeakReferenceMessenger.Default.Register<DialogueNodeViewModel, NodePinnedMessage>(this, (r, m) => { if (m.nodeId == ID) r.OnPropertyChanged(nameof(IsPinned)); });
            // TODO is this too heavy?
            WeakReferenceMessenger.Default.Register<DialogueNodeViewModel, UnsavedModificationsMessage>(this, (r, m) => r.OnPropertyChanged(nameof(NodeChildren)));

            Prompts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsPromptsEmpty));
            MetaValues.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsMetaDataEmpty));
        }

        private void UpdateParent(NodeMovedMessage m)
        {
            if (m.NodeMoved.ID == ID)
            {
                SetParentVm(_navigationService.ReuseOrCreateTreeVm(m.DestinationTree));
            }
        }

        public ObservableCollection<DialoguePartViewModel> Dialogs { get; } = new ObservableCollection<DialoguePartViewModel>();
        public ObservableCollection<ReplyPromptViewModel> Prompts { get; } = new ObservableCollection<ReplyPromptViewModel>();
        public ObservableCollection<MetadataViewModel> MetaValues { get; } = new ObservableCollection<MetadataViewModel>();

        public long ID => _node.ID;

        public int WordCount => _node.CalculateWordCount();

        public bool IsPromptsEmpty => Prompts.Count == 0;
        public bool IsMetaDataEmpty => MetaValues.Count == 0;

        public bool IsTrashed => _dataService.IsNodeTrashed(_node);
        public bool IsPinned => _dataService.IsNodePinned(_node);

        public string NodeTitle
        {
            get => _node.Title;
            set => SetProperty(_node.Title, value, _node, (u, n) => {

                    var oldCount = NodeTitle?.Split(' ')?.Length ?? 0;
                    var newCount = n.Split(' ').Length;
                    u.Title = n;
                    WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage(newCount - oldCount)); 
            });
        }

        public string TreeTitle => _parentVm.Title;
        public string TextSummary => Dialogs.Select(d => d.RtfDialogueText).FirstOrDefault();

        private void UpdateTextSummary(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(TextSummary));

        [RelayCommand]
        private void OpenSettings() => _navigationService.Navigate<SettingsViewModel>();

        [RelayCommand]
        private void OpenNode(long nodeId)
        {
            var tuple = _dataService.GetNode(nodeId);

            if (tuple != null)
                _navigationService.OpenDialogueNode(tuple.Item1, tuple.Item2);
        }

        [RelayCommand]
        private void ShowInTree()
        {
            _navigationService.Navigate<DialogueTreeViewModel>(_parentVm);
            _parentVm.SelectedNode = null;
            _parentVm.SelectedNode = this;
        } 

        [RelayCommand]
        private void AddDialog(DialogueText text = null)
        {
            if (text == null)
            {
                text = new DialogueText();
                _node.DialogueLines.Add(text);
                WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());
            }

            var dialogVm = DialoguePartViewModel.Create(text, this);
            ActivateDialogue(dialogVm);
            dialogVm.PropertyChanged += UpdateTextSummary;

            Dialogs.Add(dialogVm);
        }

        public List<DialogueNodeViewModel> NodeChildren => GetNodesLinkedByUs().Select(n => Create(n, _parentVm)).ToList();

        public List<DialogueNode> GetNodesLinkedByUs()
        {
            return Prompts.Select(p => p.LinkedID)
                .Where(i => i!= 0 && i!= ID) // Filter out no linked IDs and ourselves
                .Select(i => _dataService.GetNode(i)?.Item2) // Get DialogueNode
                .Where(y => y != null) //Check for nulls in case a linked node was deleted
                .ToList();
        }

        public List<DialogueNode> GetNodesLinkingToUs()
        {
            return _dataService.GetNodesLinkingToID(ID);
        }

        public void RemoveDialog(DialoguePartViewModel vm, DialogueText matchingText)
        {
            vm.PropertyChanged -= UpdateTextSummary;
            _node.DialogueLines.Remove(matchingText);
            Dialogs.Remove(vm);

            Dialogs.First().IsActive = true;
            WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());
        }

        [RelayCommand]
        private void AddPrompt() => AddPrompt(null);

        private void AddPrompt(DialogueReply reply = null)
        {
            if (reply == null)
            {
                reply = new DialogueReply();
                _node.Prompts.Add(reply);
                WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());
            }

            var promptVm = ReplyPromptViewModel.Create(reply, this);

            Prompts.Add(promptVm);
        }

        public void RemovePrompt(ReplyPromptViewModel vm, DialogueReply matchingReply)
        {
            _node.Prompts.Remove(matchingReply);
            Prompts.Remove(vm);
            WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());
        }

        private void AddMetadata(DialogueMetadataValue metaval)
        {
            MetaValues.Add(new MetadataViewModel(metaval));
        }

        [RelayCommand]
        private void PinDialogue() => WeakReferenceMessenger.Default.Send(new AskToPinNodeMessage(_node, true));       

        [RelayCommand]
        private void UnpinDialogue() => WeakReferenceMessenger.Default.Send(new AskToPinNodeMessage(_node, false));

        [RelayCommand]
        private void MoveToTrash()
        {
            _parentVm.MoveNodeToTrash(this, _node);
            _notificationService.ShowInAppNotification(Resources.NotificationTrashed);
        } 

        [RelayCommand]
        private async Task Delete()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogueDeleteNode, Resources.ContentDialogWillBePermaDeleted,
                        Resources.ButtonYesText, Resources.ButtonCancelText))
            {
                DeleteNode();
            }
        }

        internal void DeleteNode()
        {
            _parentVm.DeleteNode(this, _node);
            _navigationService.CloseDialogueNode(this);
        }

        internal void SetParentVm(DialogueTreeViewModel treeViewModel)
        {
            _parentVm = treeViewModel;
            OnPropertyChanged(nameof(TreeTitle));
        }

        internal void ActivateDialogue(DialoguePartViewModel dialogVm)
        {
            foreach (var vm in Dialogs.ToList())
            {
                vm.IsActive = false;
            }

            dialogVm.IsActive = true;
        }
    }
}
