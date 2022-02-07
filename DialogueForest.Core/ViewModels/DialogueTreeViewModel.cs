using System;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DialogueForest.Core.ViewModels
{
    public class DialogueTreeViewModel : TreeViewModelBase
    {
        public DialogueTreeViewModel(IDialogService dialogService, IInteropService interopService, INavigationService notificationService, ForestDataService forestService) :
            base(dialogService, interopService, notificationService, forestService)
        {
        }
    }
}
