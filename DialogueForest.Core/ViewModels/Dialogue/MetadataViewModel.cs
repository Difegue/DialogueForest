using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.ViewModels
{
    /// <summary>
    /// This VM is on double duty, serving as both settings and actual dataobject value.
    /// </summary>
    public partial class MetadataViewModel : ObservableObject
    {
        private SettingsViewModel _parentVm;
        private DialogueMetadataValue _metaval;

        public MetadataViewModel(SettingsViewModel parent)
        {
            _parentVm = parent;
            _metaval = new DialogueMetadataValue(); // Unlinked metaval so we can still use SetProperty
        }

        public MetadataViewModel(DialogueMetadataValue metaval)
        {
            _metaval = metaval;
            _kind = metaval.Kind; // Kind is immutable in this case
        }

        [ObservableProperty]
        public MetadataKind _kind;

        public string Name
        {
            get => _metaval.Key;
            set => SetProperty(_metaval.Key, value, _metaval, (u, n) => { u.Key = n; 
                WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage()); 
            });
        }

        public object Value
        {
            get => _metaval.Value;
            set => SetProperty(_metaval.Value, value, _metaval, (u, n) => { u.Value = n; 
                WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage()); 
            });
        }        

        [ICommand]
        private void Delete()
        {
            _parentVm?.RemoveMetadata(this);
        }
    }
}
