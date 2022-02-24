using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Core.ViewModels;
using DialogueForest.Localization.Strings;
using SkiaSharp;

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

        internal static DialogueNodeViewModel Create(DialogueNode node)
        {
            var instance = Ioc.Default.GetRequiredService<DialogueNodeViewModel>();
            instance.LoadFromNode(node);

            return instance;
        }

        public void Receive(ForestSettingsChangedMessage message)
        {
            // Metadata definitions mightve changed, reload data from node
            UpdateMetadata();
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

            WeakReferenceMessenger.Default.Register<DialogueNodeViewModel, ForestSettingsChangedMessage>(this, (r, m) => r.Receive(m));

            Prompts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsPromptsEmpty));
            MetaValues.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsMetaDataEmpty));
        }

        public ObservableCollection<DialoguePartViewModel> Dialogs { get; } = new ObservableCollection<DialoguePartViewModel>();
        public ObservableCollection<ReplyPromptViewModel> Prompts { get; } = new ObservableCollection<ReplyPromptViewModel>();
        public ObservableCollection<MetadataViewModel> MetaValues { get; } = new ObservableCollection<MetadataViewModel>();

        public long ID => _node.ID;

        public bool IsPromptsEmpty => Prompts.Count == 0;
        public bool IsMetaDataEmpty => MetaValues.Count == 0;

        public bool IsTrashed => _dataService.IsNodeTrashed(_node);
        public bool IsPinned => _dataService.IsNodePinned(_node);

        public string NodeTitle
        {
            get => _node.Title;
            set => SetProperty(_node.Title, value, _node, (u, n) => u.Title = n);
        }

        public string TextSummary => Dialogs.Select(d => d.RtfDialogueText).FirstOrDefault();

        private void UpdateTextSummary(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(TextSummary));

        [ICommand]
        private void OpenSettings() => _navigationService.Navigate<SettingsViewModel>();

        [ICommand]
        private void ShowInTree()
        {
            _navigationService.Navigate<DialogueTreeViewModel>(_parentVm);
            _parentVm.SelectedNode = null;
            _parentVm.SelectedNode = this;
        } 

        [ICommand]
        private void AddDialog(DialogueText text = null)
        {
            if (text == null)
            {
                text = new DialogueText();
                _node.DialogueLines.Add(text);
            }

            var dialogVm = DialoguePartViewModel.Create(text, this);
            ActivateDialogue(dialogVm);
            dialogVm.PropertyChanged += UpdateTextSummary;

            Dialogs.Add(dialogVm);
        }

        public void RemoveDialog(DialoguePartViewModel vm, DialogueText matchingText)
        {
            vm.PropertyChanged -= UpdateTextSummary;
            _node.DialogueLines.Remove(matchingText);
            Dialogs.Remove(vm);
        }

        [ICommand]
        private void AddPrompt() => AddPrompt(null);

        private void AddPrompt(DialogueReply reply = null)
        {
            if (reply == null)
            {
                reply = new DialogueReply();
                _node.Prompts.Add(reply);
            }

            var promptVm = ReplyPromptViewModel.Create(reply, this);

            Prompts.Add(promptVm);
        }

        public void RemovePrompt(ReplyPromptViewModel vm, DialogueReply matchingReply)
        {
            _node.Prompts.Remove(matchingReply);
            Prompts.Remove(vm);
        }

        private void AddMetadata(DialogueMetadataValue metaval)
        {
            MetaValues.Add(new MetadataViewModel(metaval));
        }

        [ICommand]
        private void PinDialogue()
        {
            _dataService.SetPinnedNode(_node, true);
            _notificationService.ShowInAppNotification("Pinned!");
        }
        

        [ICommand]
        private void UnpinDialogue() => _dataService.SetPinnedNode(_node, false);

        [ICommand]
        private void MoveToTrash()
        {
            _parentVm.MoveNodeToTrash(this, _node);
            _notificationService.ShowInAppNotification("Trashed!");
        } 

        [ICommand]
        private async Task Delete()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogueDeleteNode, Resources.ContentDialogWillBePermaDeleted,
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _dataService.DeleteNode(_node);
        }

        internal void SetParentVm(DialogueTreeViewModel treeViewModel)
        {
            _parentVm = treeViewModel;
        }

        internal void ActivateDialogue(DialoguePartViewModel dialogVm)
        {
            foreach (var vm in Dialogs)
            {
                vm.IsActive = false;
            }

            dialogVm.IsActive = true;
        }
    }
}
