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

namespace DialogueForest.ViewModels
{

    public partial class DialogueNodeViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private IInteropService _interopService;
        private INotificationService _notificationService;
        private ForestDataService _dataService;

        private TreeViewModelBase _parentVm;
        private DialogueNode _node;

        internal static DialogueNodeViewModel Create(DialogueNode node)
        {
            var instance = Ioc.Default.GetRequiredService<DialogueNodeViewModel>();
            instance.LoadFromNode(node);

            return instance;
        }

        public DialogueNodeViewModel(IDialogService dialogService, IInteropService interopService, INotificationService notificationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _interopService = interopService;
            _dataService = forestService;


            AddDialog();
            Prompts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsPromptsEmpty));
            MetaValues.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsMetaDataEmpty));
        }

        public ObservableCollection<DialoguePartViewModel> Dialogs { get; } = new ObservableCollection<DialoguePartViewModel>();
        public ObservableCollection<ReplyPromptViewModel> Prompts { get; } = new ObservableCollection<ReplyPromptViewModel>();
        public ObservableCollection<MetadataViewModel> MetaValues { get; } = new ObservableCollection<MetadataViewModel>();

        public long ID => _node.ID;

        public bool IsPromptsEmpty => Prompts.Count == 0;
        public bool IsMetaDataEmpty => MetaValues.Count == 0;

        [ObservableProperty]
        private string _nodeTitle;
        [ObservableProperty]
        private bool _isTrashed;
        [ObservableProperty]
        private bool _isPinned;

        public string PlainText => Dialogs.FirstOrDefault()?.PlainDialogueText;

        // TODO this might be changed so that the plainText only updates when the node is saved
        private void UpdatePlainText(object sender, PropertyChangedEventArgs e) => OnPropertyChanged(nameof(PlainText));

        [ICommand]
        private void AddDialog(string text = null)
        {
            var dialogVm = Ioc.Default.GetRequiredService<DialoguePartViewModel>();
            dialogVm.SetParentVm(this);
            ActivateDialogue(dialogVm);

            if (text != null)
                dialogVm.RtfDialogueText = text;

            dialogVm.PropertyChanged += UpdatePlainText;

            Dialogs.Add(dialogVm);
        }

        public void RemoveDialog(DialoguePartViewModel vm)
        {
            vm.PropertyChanged -= UpdatePlainText;
            Dialogs.Remove(vm);
        }

        [ICommand]
        private void AddPrompt() => AddPrompt(null);

        private void AddPrompt(string text = null, long replyID = -1)
        {
            var promptVm = Ioc.Default.GetRequiredService<ReplyPromptViewModel>();
            promptVm.SetParentVm(this);

            if (text != null)
                promptVm.ReplyText = text;

            if (replyID != -1)
                promptVm.LinkedID = replyID;

            Prompts.Add(promptVm);
        }

        public void RemovePrompt(ReplyPromptViewModel vm) => Prompts.Remove(vm);

        [ICommand]
        private void PinDialogue()
        {

        }

        [ICommand]
        private void MoveToTrash()
        {

        }

        [ICommand]
        private void SaveNode()
        {

        }

        internal void SetParentVm(TreeViewModelBase treeViewModel)
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
        private void LoadFromNode(DialogueNode node)
        {
            _node = node;
            _nodeTitle = node.Title;

            // TODO character/metadata

            foreach (var prompt in node.Prompts) 
                AddPrompt(prompt.Key, prompt.Value);

            foreach (var dialogue in node.DialogueLines)
                AddDialog(dialogue);
        }
    }
}
