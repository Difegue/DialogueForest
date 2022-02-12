using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;

namespace DialogueForest.Core.ViewModels
{
    public partial class WelcomeViewModel : ObservableObject
    {

        [ICommand]
        private void NewFile()
        {

        }

        [ICommand]
        private void OpenFile()
        {

        }

        private IApplicationStorageService _applicationStorageService;
        private IInteropService _interop;

        [ObservableProperty]
        private string _welcomeText;

        public WelcomeViewModel(IApplicationStorageService appStorage, IInteropService interop)
        {
            _applicationStorageService = appStorage;
            _interop = interop;
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
