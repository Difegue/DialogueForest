﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.ViewModels
{

    public partial class MetadataViewModel : ObservableObject
    {
        private SettingsViewModel _parentVm;

        public MetadataViewModel(SettingsViewModel parent)
        {
            _parentVm = parent;
        }

        [ObservableProperty]
        private MetadataKind _kind;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private object _value;

        [ICommand]
        private void Delete()
        {
            _parentVm.RemoveMetadata(this);
        }
    }
}
