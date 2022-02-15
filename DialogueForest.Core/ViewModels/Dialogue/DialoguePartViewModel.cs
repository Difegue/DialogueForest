using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.Core.ViewModels
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

        public List<string> Characters;

        public string RtfDialogueText
        {
            get => _text.RichText;
            set => SetProperty(_text.RichText, value, _text, (u, n) => u.RichText = n);
        }

        public string CharacterName
        {
            get => _text.Character;
            set => SetProperty(_text.Character, value, _text, (u, n) => u.Character = n);
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
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogDeleteDialogue, Resources.ContentDialogWillBePermaDeleted,
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemoveDialog(this, _text);
        }

        public DialoguePartViewModel(IDialogService dialogService, ForestDataService dataService)
        {
            _dialogService = dialogService;
            Characters = dataService.GetCharacters(); //TODO update character list if settings updated
        }
    }
}
