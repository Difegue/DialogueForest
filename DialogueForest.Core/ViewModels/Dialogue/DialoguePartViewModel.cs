using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Localization.Strings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.ViewModels
{
    public partial class DialoguePartViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _rtfDialogueText;

        [ObservableProperty]
        private string _plainDialogueText;

        [ObservableProperty]
        private bool _isActive;

        private DialogueNodeViewModel _parentNodeVm;

        private IInteropService _interopService;
        private IDialogService _dialogService;

        public DialoguePartViewModel(IDialogService dialogService, IInteropService interopService)
        {
            _dialogService = dialogService;
            _interopService = interopService;
        }

        public void SetParentVm(DialogueNodeViewModel parent) => _parentNodeVm = parent;

        [ICommand]
        private void Activate() => _parentNodeVm.ActivateDialogue(this);

        [ICommand]
        private async Task Remove()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.DeletePlaylistContentDialog, Resources.EmptySearchDesc,
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemoveDialog(this);
        }

    }
}
