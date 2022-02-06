using System;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{
    public class PinsViewModel : TreeViewModelBase
    {
        public PinsViewModel(IDialogService dialogService, IInteropService interopService, INotificationService notificationService, ForestDataService forestService) :
            base(dialogService, interopService, notificationService, forestService)
        {
        }

    }
}
