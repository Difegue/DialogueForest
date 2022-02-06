using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using System;
using System.Collections.Generic;
using DialogueForest.Localization.Strings;
using System.Threading.Tasks;
using DialogueForest.Core.Services;

namespace DialogueForest.ViewModels
{
    public partial class ReplyPromptViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _replyText;

        [ObservableProperty]
        private long _linkedID;

        private DialogueNodeViewModel _parentNodeVm;
        private IDialogService _dialogService;
        private INavigationService _navigationService;
        private ForestDataService _dataService;

        [ICommand]
        private async Task Remove()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.DeletePlaylistContentDialog, Resources.EmptySearchDesc, 
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemovePrompt(this);
        }

        [ICommand]
        private void GoToLinkedDialogue()
        {
            if (_linkedID < 0) return;

            var node = _dataService.GetNode(_linkedID);

            if (node != null)
                _navigationService.OpenDialogueNode(node);
        }

        public void SetParentVm(DialogueNodeViewModel parent) => _parentNodeVm = parent;

        public ReplyPromptViewModel(ForestDataService dataService, IDialogService dialogService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

    }
}
