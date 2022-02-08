using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Localization.Strings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.ViewModels
{
    public partial class DialoguePartViewModel : ObservableObject
    {
        private IDialogService _dialogService;

        protected DialogueNodeViewModel _parentNodeVm;
        protected DialogueText _text;

        internal static DialoguePartViewModel Create(DialogueText text, DialogueNodeViewModel parent)
        {
            var instance = Ioc.Default.GetRequiredService<DialoguePartViewModel>();
            instance._text = text;
            instance._parentNodeVm = parent;

            return instance;
        }

        public string RtfDialogueText
        {
            get => _text.RichText;
            set => SetProperty(_text.RichText, value, _text, (u, n) => u.RichText = n);
        }

        [ObservableProperty]
        private string _plainDialogueText;

        [ObservableProperty]
        private bool _isActive;

        [ICommand]
        private void Activate() => _parentNodeVm.ActivateDialogue(this);

        [ICommand]
        private async Task Remove()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.DeletePlaylistContentDialog, Resources.EmptySearchDesc,
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemoveDialog(this, _text);
        }

        public DialoguePartViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
    }
}
