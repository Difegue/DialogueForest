using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.Core.ViewModels
{
    public partial class DialoguePartViewModel : ObservableObject
    {
        private IDialogService _dialogService;
        private ForestDataService _dataService;
        private DialogueNodeViewModel _parentNodeVm;
        private DialogueText _text;

        internal static DialoguePartViewModel Create(DialogueText text, DialogueNodeViewModel parent)
        {
            var instance = Ioc.Default.GetRequiredService<DialoguePartViewModel>();
            instance._text = text;
            instance.SetParent(parent);

            return instance;
        }

        private void SetParent(DialogueNodeViewModel parent)
        {
            _parentNodeVm = parent;
            _parentNodeVm.Dialogs.CollectionChanged += (s, e) => 
                OnPropertyChanged(nameof(ParentHasMultipleDialogs));
        }

        public ObservableCollection<string> Characters = new ObservableCollection<string>();

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

        public bool ParentHasMultipleDialogs => _parentNodeVm.Dialogs.Count > 1;

        public void Receive(ForestSettingsChangedMessage message)
        {
            //TODO keep CharacterName even if it's not in the list (seems to be a display issue)

            // Update character list if settings updated
            Characters.Clear();
            foreach (var c in _dataService.GetCharacters())
                Characters.Add(c);
        }

        public DialoguePartViewModel(IDialogService dialogService, ForestDataService dataService)
        {
            _dialogService = dialogService;
            _dataService = dataService;

            Characters.Clear();
            foreach (var c in _dataService.GetCharacters())
                Characters.Add(c);

            WeakReferenceMessenger.Default.Register<DialoguePartViewModel, ForestSettingsChangedMessage>(this, (r, m) => r.Receive(m));
        }
    }
}
