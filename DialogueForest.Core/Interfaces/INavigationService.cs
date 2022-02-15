using CommunityToolkit.Mvvm.ComponentModel;
using DialogueForest.Core.Models;
using DialogueForest.Core.ViewModels;
using System;

namespace DialogueForest.Core.Interfaces
{
    public class CoreNavigationEventArgs : EventArgs { public Type NavigationTarget { get; set; } public object Parameter { get; set; } }

    public interface INavigationService
    {
        Type CurrentPageViewModelType { get; }
        bool CanGoBack { get; }
        bool GoBack();

        void Navigate(Type viewmodel, object parameter = null);
        void Navigate<T>(object parameter = null) where T : ObservableObject;
        void SetItemForNextConnectedAnimation(object item);
        void OpenDialogueNode(DialogueNode node);
        void OpenDialogueNode(DialogueNodeViewModel existingVm);
    }

    public abstract class NavigationServiceBase: INavigationService
    {
        public event EventHandler<CoreNavigationEventArgs> Navigated;

        public void Navigate<T>(object parameter = null) where T : ObservableObject => Navigate(typeof(T), parameter);
        public void Navigate(Type viewmodel, object parameter = null)
        {
            if (viewmodel == null) return;

            ShowViewModel(viewmodel, parameter);
            Navigated?.Invoke(this, new CoreNavigationEventArgs { NavigationTarget = viewmodel, Parameter = parameter});
        }
        public abstract void ShowViewModel(Type viewmodel, object parameter = null);

        public bool GoBack()
        {
            var result = GoBackImplementation();

            if (result != null)
            {
                // Get the viewmodel we landed back on from the implementation, and send an event with it
                Navigated?.Invoke(this, new CoreNavigationEventArgs { NavigationTarget = CurrentPageViewModelType, Parameter = result });
                return true;
            }
            return false;
        }

        public void OpenDialogueNode(DialogueNode node)
        {
            var vm = DialogueNodeViewModel.Create(node);
            OpenDialogueNode(vm);
        }

        public abstract object GoBackImplementation();

        public abstract Type CurrentPageViewModelType { get; }
        public abstract bool CanGoBack { get; }
        public abstract void SetItemForNextConnectedAnimation(object item);
        public abstract void OpenDialogueNode(DialogueNodeViewModel existingVm);
    }
}
