using System;
using System.Collections.ObjectModel;
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

        internal void ActivateDialogue(DialoguePartViewModel dialogVm)
        {
            foreach (var vm in Dialogs)
            {
                vm.IsActive = false;
            }

            dialogVm.IsActive = true;
        }

        public ObservableCollection<DialoguePartViewModel> Dialogs { get; } = new ObservableCollection<DialoguePartViewModel>();
        public ObservableCollection<ReplyPromptViewModel> Prompts { get; } = new ObservableCollection<ReplyPromptViewModel>();
        public ObservableCollection<MetadataViewModel> MetaValues { get; } = new ObservableCollection<MetadataViewModel>();

        internal void SetParentVm(TreeViewModelBase treeViewModel)
        {
            _parentVm = treeViewModel;
        }

        public long ID => _node.ID;

        public bool IsPromptsEmpty => Prompts.Count == 0;
        public bool IsMetaDataEmpty => MetaValues.Count == 0;

        [ObservableProperty]
        private string _nodeTitle;
        [ObservableProperty]
        private bool _isTrashed;
        [ObservableProperty]
        private bool _isPinned;

        [ICommand]
        private void AddDialog()
        {
            var dialogVm = Ioc.Default.GetRequiredService<DialoguePartViewModel>();
            dialogVm.SetParentVm(this);
            ActivateDialogue(dialogVm);

            Dialogs.Add(dialogVm);
        }

        public void RemoveDialog(DialoguePartViewModel vm) => Dialogs.Remove(vm);

        [ICommand]
        private void AddPrompt()
        {
            var promptVm = Ioc.Default.GetRequiredService<ReplyPromptViewModel>();
            promptVm.SetParentVm(this);

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
    }
}
