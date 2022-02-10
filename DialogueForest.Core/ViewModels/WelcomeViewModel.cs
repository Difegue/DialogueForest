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


        public WelcomeViewModel(IApplicationStorageService appStorage, IInteropService interop)
        {
            _applicationStorageService = appStorage;
            _interop = interop;

            
        }
    }
}
