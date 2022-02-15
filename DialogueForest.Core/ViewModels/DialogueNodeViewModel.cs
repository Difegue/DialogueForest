using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Core.ViewModels;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{

    public partial class DialogueNodeViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private IInteropService _interopService;
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
        private void LoadFromNode(DialogueNode node)
        {
            _node = node;

            // TODO metadata

            foreach (var prompt in node.Prompts)
                AddPrompt(prompt);

            foreach (var dialogue in node.DialogueLines)
                AddDialog(dialogue);
        }

        public DialogueNodeViewModel(IDialogService dialogService, IInteropService interopService, INotificationService notificationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _interopService = interopService;
            _dataService = forestService;

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

        public string PlainText => Dialogs.FirstOrDefault()?.PlainDialogueText;

        private void UpdatePlainText(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(PlainText));

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
            dialogVm.PropertyChanged += UpdatePlainText;

            Dialogs.Add(dialogVm);
        }

        public void RemoveDialog(DialoguePartViewModel vm, DialogueText matchingText)
        {
            vm.PropertyChanged -= UpdatePlainText;
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

        [ICommand]
        private void PinDialogue() => _dataService.SetPinnedNode(_node, true);

        [ICommand]
        private void UnpinDialogue() => _dataService.SetPinnedNode(_node, false);

        [ICommand]
        private void MoveToTrash() => _parentVm.MoveNodeToTrash(this, _node);

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
