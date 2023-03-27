using DialogueForest.Core.ViewModels;
using DialogueForest.Views;
using CommunityToolkit.WinUI.UI.Animations;
using DialogueForest.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DialogueForest.Services
{
    public class NavigationService : NavigationServiceBase
    {
        private Dictionary<Type, Type> _viewModelToPageDictionary = new Dictionary<Type, Type>()
        {
            { typeof(DialogueNodeViewModel), typeof(DialogueNodePage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(DialogueTreeViewModel), typeof(DialogueTreePage) },
            { typeof(ExportViewModel), typeof(ExportPage) },
            { typeof(WelcomeViewModel), typeof(HomePage) }
        };

        public NavigationService()
        {

        }

        public override bool CanGoBack => Frame.BackStack.Count > 0;

        public override Type CurrentPageViewModelType => _viewModelToPageDictionary.Keys.Where(
            k => _viewModelToPageDictionary[k] == Frame.CurrentSourcePageType).FirstOrDefault();

        private object _lastParamUsed;
        public override void ShowViewModel(Type viewmodelType, object parameter = null)
        {
            // Get the matching page and navigate to it
            var pageType = _viewModelToPageDictionary.GetValueOrDefault(viewmodelType);

            // Don't open the same page multiple times
            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                var navigationResult = Frame.Navigate(pageType, parameter);
                if (navigationResult)
                {
                    _lastParamUsed = parameter;
                }
            }
        }

        public override void SetItemForNextConnectedAnimation(object item) => Frame.SetListDataItemForNextConnectedAnimation(item);

        public override void OpenDialogueNode(DialogueNodeViewModel vm)
        {
            // Add the previous node to the backstack
            Frame.BackStack.Add(new Microsoft.UI.Xaml.Navigation.PageStackEntry(typeof(DialogueNodePage), NodeTabContainer.SelectedItem, null));
            NodeTabContainer.OpenNode(vm);
            //TODO invoke navigated here so CanGoBack updates
        }

        public override void CloseDialogueNode(DialogueNodeViewModel vm)
        {
            NodeTabContainer.CloseNode(vm);
        }

        public override object GoBackImplementation()
        {
            if (CanGoBack)
            {
                var param = Frame.BackStack.Last().Parameter;

                // Nodes are a special case and use the tabcontainer instead of the frame
                if (param is DialogueNodeViewModel vm)
                {
                    NodeTabContainer.OpenNode(vm);
                    Frame.BackStack.Remove(Frame.BackStack.Last());
                    return null;
                }
                else
                {
                    Frame.GoBack();
                    return param;
                }
                
            }

            return null;
        }
        

        public OpenedNodesViewModel NodeTabContainer { get; set; }

        private Frame _frame;
        public Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = (Application.Current as App)?.Window.Content as Frame;
                }

                return _frame;
            }

            set
            {
                _frame = value;
            }
        }

    }
}
