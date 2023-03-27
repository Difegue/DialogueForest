using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
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
            set => SetProperty(_text.RichText, value, _text, (u, n) => { u.RichText = n; 
                    WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage()); 
            });
        }

        public string CharacterName
        {
            get => _text?.Character;
            set => SetProperty(_text.Character, value, _text, (u, n) => { u.Character = n; 
                    WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage()); 
            });
        }

        [ObservableProperty]
        private bool _isActive;

        [RelayCommand]
        private void Activate() => _parentNodeVm.ActivateDialogue(this);

        [RelayCommand]
        private async Task Remove()
        {
            if (await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogDeleteDialogue, Resources.ContentDialogWillBePermaDeleted,
                        Resources.ButtonYesText, Resources.ButtonCancelText))
                _parentNodeVm.RemoveDialog(this, _text);
        }

        public bool ParentHasMultipleDialogs => _parentNodeVm.Dialogs.Count > 1;

        public void UpdateCharacters(ForestSettingsChangedMessage message)
        {
            Characters.Clear();
                
            foreach (var c in _dataService.GetCharacters())
            {
                Characters.Add(c);
            }
        }

        public DialoguePartViewModel(IDialogService dialogService, ForestDataService dataService)
        {
            _dialogService = dialogService;
            _dataService = dataService;

            // Fill the list of characters
            UpdateCharacters(null);
            WeakReferenceMessenger.Default.Register<DialoguePartViewModel, ForestSettingsChangedMessage>(this, (r, m) => r.UpdateCharacters(m));
        }
    }
}
