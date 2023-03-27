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
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Messages;

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

            return instance;
        }

        public List<long> LinkableIDs => _parentNodeVm.GetIDs();

        public string ReplyText
        {
            get => _reply.Text;
            set => SetProperty(_reply.Text, value, _reply, (u, n) => { u.Text = n; 
                    WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage()); 
            });
        }

        public bool HasLinkedID => LinkedID > 0 && LinkedID != _parentNodeVm.ID;
        public long LinkedID
        {
            get => _reply.LinkedID;
            set
            {
                SetProperty(_reply.LinkedID, value, _reply, (u, n) => { u.LinkedID = n;
                    WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());
                });
                OnPropertyChanged(nameof(HasLinkedID));
            }
        }


        [RelayCommand]
        private async Task Remove()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogDeleteReply, Resources.ContentDialogWillBePermaDeleted, 
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemovePrompt(this, _reply);
        }

        [RelayCommand]
        private void GoToLinkedDialogue()
        {
            if (LinkedID < 0 || LinkedID == _parentNodeVm.ID) return;

            var tuple = _dataService.GetNode(LinkedID);

            if (tuple != null)
                _navigationService.OpenDialogueNode(tuple.Item1, tuple.Item2);
        }

        public ReplyPromptViewModel(ForestDataService dataService, IDialogService dialogService, INavigationService navigationService)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _dialogService = dialogService;
        }

    }
}
