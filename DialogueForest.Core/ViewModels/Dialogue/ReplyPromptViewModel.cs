using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using System;
using System.Collections.Generic;
using DialogueForest.Localization.Strings;
using System.Threading.Tasks;
using DialogueForest.Core.Services;
using DialogueForest.Core.Models;
using CommunityToolkit.Mvvm.DependencyInjection;

namespace DialogueForest.Core.ViewModels
{
    public partial class ReplyPromptViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private INavigationService _navigationService;
        private ForestDataService _dataService;

        protected DialogueNodeViewModel _parentNodeVm;
        protected DialogueReply _reply;

        internal static ReplyPromptViewModel Create(DialogueReply reply, DialogueNodeViewModel parent)
        {
            var instance = Ioc.Default.GetRequiredService<ReplyPromptViewModel>();
            instance._reply = reply;
            instance._parentNodeVm = parent;

            instance.LinkableIDs = parent.GetIDs();

            return instance;
        }

        public List<long> LinkableIDs { get; private set; }
        public bool HasLinkedID => LinkedID > 0 && LinkedID != _parentNodeVm.ID;

        public string ReplyText
        {
            get => _reply.Text;
            set => SetProperty(_reply.Text, value, _reply, (u, n) => u.Text = n);
        }

        public long LinkedID
        {
            get => _reply.LinkedID;
            set
            {
                SetProperty(_reply.LinkedID, value, _reply, (u, n) => u.LinkedID = n);
                OnPropertyChanged(nameof(HasLinkedID));
            }
        }


        [ICommand]
        private async Task Remove()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogDeleteReply, Resources.ContentDialogWillBePermaDeleted, 
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemovePrompt(this, _reply);
        }

        [ICommand]
        private void GoToLinkedDialogue()
        {
            if (LinkedID < 0) return;

            var node = _dataService.GetNode(LinkedID);

            if (node != null)
                _navigationService.OpenDialogueNode(node);
        }

        public ReplyPromptViewModel(ForestDataService dataService, IDialogService dialogService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

    }
}
