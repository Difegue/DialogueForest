using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{
    public partial class WelcomeViewModel : ObservableObject
    {

        [RelayCommand]
        private async Task NewFile()
        {
            if (_dataService.CurrentForestHasUnsavedChanges)
            {
                var confirm = await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogUnsavedChanges, Resources.ContentDialogUnsavedChangesText,
                    Resources.ButtonYesText, Resources.ButtonCancelText);

                if (!confirm) return;
            }

            _dataService.ResetDatabase();
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            if (_dataService.CurrentForestHasUnsavedChanges)
            {
                var confirm = await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogUnsavedChanges, Resources.ContentDialogUnsavedChangesText, 
                    Resources.ButtonYesText, Resources.ButtonCancelText);

                if (!confirm) return;
            }

            await _dataService.LoadForestFromFileAsync();
        }

        [RelayCommand]
        private async Task SaveFileAs() => await _dataService.SaveForestToFileAsync(true);

        [RelayCommand]
        private async Task ShowWhatsNew() => await _dialogService.ShowWhatsNewDialogIfAppropriateAsync(true);

        [ObservableProperty]
        private string _welcomeText;

        [ObservableProperty]
        private string _totalWordCount;

        [ObservableProperty]
        private string _totalDialogues;

        [ObservableProperty]
        private string _deletedDialogues;

        private ForestDataService _dataService;
        private IDialogService _dialogService;

        public WelcomeViewModel(ForestDataService dataService, IDialogService dialogService)
        {
            _dataService = dataService;
            _dialogService = dialogService;

            WeakReferenceMessenger.Default.Register<WelcomeViewModel, TreeUpdatedMessage>(this, (r, m) =>
            {
                UpdateForestStats();
            });
        }

        public void UpdateWelcomeText()
        {
            var dt = DateTime.Now;
            int hours = dt.Hour;
            
            if (hours > 6 && hours < 12)
            {
                WelcomeText = Resources.WelcomeGreetingMorning;
            }
            else if (hours > 12 && hours < 16)
            {
                WelcomeText = Resources.WelcomeGreetingAfternoon;
            }
            else if (hours > 16 && hours < 21)
            {
                WelcomeText = Resources.WelcomeGreetingEvening;
            }
            else 
            {
                WelcomeText = Resources.WelcomeGreetingNight;
            }

        }

        public void UpdateForestStats()
        {
            int dialogues = 0;
            int words = 0;

            if (_dataService.GetDialogueTrees() == null)
                return;

            var lastId = _dataService.GetLastID();
            var trees = new List<DialogueTree>(_dataService.GetDialogueTrees());
            trees.Add(_dataService.GetNotes());
            trees.Add(_dataService.GetTrash());

            foreach (var t in trees)
            {
                dialogues += t.Nodes.Count;

                foreach (var n in t.Nodes)
                {
                    words += n.Value.CalculateWordCount();
                }
            }

            TotalDialogues = string.Format(Resources.WelcomeTotalDialogueCount, dialogues.ToString("n0"));
            TotalWordCount = string.Format(Resources.WelcomeTotalWordCount, words.ToString("n0"));
            DeletedDialogues = string.Format(Resources.WelcomeDeletedCount, lastId - dialogues);
        }
    };
}
