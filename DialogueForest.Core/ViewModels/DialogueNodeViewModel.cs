using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;

namespace DialogueForest.ViewModels
{

    public partial class DialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _text;

        [ObservableProperty]
        private bool _isActive;
    }

    public partial class MetadataViewModel : ObservableObject
    {

        [ObservableProperty]
        private MetadataKind _kind;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private object _value;

    }

    public partial class ReplyPromptViewModel: ObservableObject
    {
        [ObservableProperty]
        private string _replyText;

        [ObservableProperty]
        private long _linkedID;
    }

    public partial class DialogueNodeViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private INotificationService _notificationService;
        private ForestDataService _forestService;

        public DialogueNodeViewModel(IDialogService dialogService, INotificationService notificationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _forestService = forestService;

            //Source.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsSourceEmpty));
        }

        public ObservableCollection<DialogViewModel> Dialogs { get; } = new ObservableCollection<DialogViewModel>();
        public ObservableCollection<ReplyPromptViewModel> Prompts { get; } = new ObservableCollection<ReplyPromptViewModel>();
        public ObservableCollection<MetadataViewModel> MetaValues { get; } = new ObservableCollection<MetadataViewModel>();

        [ObservableProperty]
        private string _nodeTitle;

        [ObservableProperty]
        private bool _isTrashed;
        [ObservableProperty]
        private bool _isPinned;

        [ICommand]
        private void AddDialog()
        {
            
        }

        [ICommand]
        private void AddPrompt()
        {

        }

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
