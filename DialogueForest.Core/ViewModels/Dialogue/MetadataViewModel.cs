using CommunityToolkit.Mvvm.ComponentModel;
using DialogueForest.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.ViewModels
{

    public partial class MetadataViewModel : ObservableObject
    {

        [ObservableProperty]
        private MetadataKind _kind;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private object _value;

    }
}
