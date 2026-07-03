using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Messages;
using DialogueForest.Core.Models;

namespace DialogueForest.Core.ViewModels
{
    /// <summary>
    /// Wraps a <see cref="DialogueMarker"/> for display in the ordered node list. 
    /// Constructed directly by the parent tree VM (no DI needed).
    /// </summary>
    public partial class MarkerViewModel : ObservableObject, ITreeListItem
    {
        private readonly DialogueMarker _marker;
        private readonly DialogueTreeViewModel _parentVm;

        public MarkerViewModel(DialogueMarker marker, DialogueTreeViewModel parentVm)
        {
            _marker = marker;
            _parentVm = parentVm;
        }

        public bool IsMarker => true;

        internal DialogueMarker Model => _marker;

        public string Label
        {
            get => _marker.Label;
            set
            {
                if (SetProperty(_marker.Label, value, _marker, (u, n) => u.Label = n))
                    WeakReferenceMessenger.Default.Send(new UnsavedModificationsMessage());
            }
        }

        [RelayCommand]
        private void MoveUp() => _parentVm.MoveMarkerRelative(this, -1);

        [RelayCommand]
        private void MoveDown() => _parentVm.MoveMarkerRelative(this, +1);

        [RelayCommand]
        private void Delete() => _parentVm.DeleteMarker(this);
    }
}
