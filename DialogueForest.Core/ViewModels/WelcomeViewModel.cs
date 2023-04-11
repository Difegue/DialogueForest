using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
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


        private ForestDataService _dataService;
        private IDialogService _dialogService;

        public WelcomeViewModel(ForestDataService dataService, IDialogService dialogService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
        }

        public void UpdateWelcomeText()
        {
            var dt = DateTime.Now;
            int hours = dt.Hour;

            if (hours > 6 && hours < 12)
            {
                _welcomeText = Localization.Strings.Resources.WelcomeGreetingMorning;
            }
            else if (hours > 12 && hours < 16)
            {
                _welcomeText = Localization.Strings.Resources.WelcomeGreetingAfternoon;
            }
            else if (hours > 16 && hours < 21)
            {
                _welcomeText = Localization.Strings.Resources.WelcomeGreetingEvening;
            }
            else 
            {
                _welcomeText = Localization.Strings.Resources.WelcomeGreetingNight;
            }

        }
    }
}
