using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Models;
using DialogueForest.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.ViewModels
{

    public partial class CharacterViewModel : ObservableObject
    {
        private SettingsViewModel _parentVm;

        public CharacterViewModel(SettingsViewModel parent)
        {
            _parentVm = parent;
        }

        [ObservableProperty]
        private string _name;


        [RelayCommand]
        private void Delete()
        {
            _parentVm.RemoveCharacter(this);
        }
    }
}
