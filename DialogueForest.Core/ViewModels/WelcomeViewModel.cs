using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Services;

namespace DialogueForest.Core.ViewModels
{
    public partial class WelcomeViewModel : ObservableObject
    {

        [ICommand]
        private void NewFile()
        {

        }

        [ICommand]
        private async Task OpenFile() => await _dataService.LoadForestFromFileAsync();

        [ICommand]
        private async Task SaveFileAs() => await _dataService.SaveForestToFileAsync(true);

        [ICommand]
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
