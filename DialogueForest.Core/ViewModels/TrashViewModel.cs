using System;

using CommunityToolkit.Mvvm.ComponentModel;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Services;

namespace DialogueForest.Core.ViewModels
{
    public class TrashViewModel : TreeViewModelBase
    {
        public TrashViewModel(IDialogService dialogService, IInteropService interopService, INavigationService notificationService, ForestDataService forestService): 
            base(dialogService, interopService, notificationService, forestService)
        {
        }
    }
}
