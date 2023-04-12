using DialogueForest.Core.ViewModels;
using DialogueForest.Views;
using CommunityToolkit.WinUI.UI.Animations;
using DialogueForest.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection.Metadata;
using DialogueForest.Core.Models;

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
            { typeof(WelcomeViewModel), typeof(HomePage) },
            { typeof(PinnedNodesViewModel), typeof(PinsPage) }
        };

        public NavigationService()
        {

        }

        public override bool CanGoBack => Frame.BackStack.Count > 0;

        // Specific case for Dialogue Nodes as they're not opened using Frame.Navigate
        public override Type CurrentPageViewModelType => _currentFocusedVm is DialogueNodeViewModel ? typeof(DialogueNodePage) :
            _viewModelToPageDictionary.Keys.Where(k => _viewModelToPageDictionary[k] == Frame.CurrentSourcePageType).FirstOrDefault();

        private object _lastParamUsed; // Only the actual frame parameter
        private object _currentFocusedVm; // Includes DialogueNodeVMs
        public override void ShowViewModel(Type viewmodelType, object parameter = null)
        {
            // Get the matching page and navigate to it
            var pageType = _viewModelToPageDictionary.GetValueOrDefault(viewmodelType);

            // Avoid navigating to the same tree if we can help it (sometimes the parameter is a tree, sometimes a treevm...)
            // Still update the focusedVm so that CurrentPageViewModelType is correct
            if (parameter is DialogueTree t && _lastParamUsed is DialogueTreeViewModel vm)
                if (!vm.GetIDs().Except(t.Nodes.Keys).Any())
                {
                    _currentFocusedVm = vm;
                    return;
                }
            if (parameter is DialogueTreeViewModel vmp && _lastParamUsed is DialogueTree t2)
                if (!vmp.GetIDs().Except(t2.Nodes.Keys).Any())
                {
                    _currentFocusedVm = vmp;
                    return;
                }
            if (parameter is DialogueTreeViewModel vm1 && _lastParamUsed is DialogueTreeViewModel vm2)
                if (!vm1.GetIDs().Except(vm2.GetIDs()).Any())
                {
                    _currentFocusedVm = vm2;
                    return;
                }

            // Don't open the same page multiple times
            if (Frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParamUsed)))
            {
                var navigationResult = Frame.Navigate(pageType, parameter);
                if (navigationResult)
                {
                    _lastParamUsed = parameter;
                }
            }
            _currentFocusedVm = parameter;
        }

        public override void SetItemForNextConnectedAnimation(object item) => Frame.SetListDataItemForNextConnectedAnimation(item);

        public override DialogueTreeViewModel ReuseOrCreateTreeVm(DialogueTree t)
        {
            // Try to find an existing VM for this tree
            var existingVm = Frame.BackStack.Select(b => b.Parameter).OfType<DialogueTreeViewModel>()
                .FirstOrDefault(vm => !vm.GetIDs().Except(t.Nodes.Keys).Any()); // We check for equality by verifying both threes have the same node IDs

            if (existingVm == null)
            {
                // If we didn't find one, create a new one
                existingVm = DialogueTreeViewModel.Create(t);
            }

            return existingVm;
        }

        public override void OpenDialogueNode(DialogueNodeViewModel vm)
        {
            if (_currentFocusedVm == vm && !NodeTabContainer.NoTabsOpen) return;

            // Add the previous node to the backstack
            if (NodeTabContainer.SelectedItem != null)
                Frame.BackStack.Add(new Microsoft.UI.Xaml.Navigation.PageStackEntry(typeof(DialogueNodePage), NodeTabContainer.SelectedItem, null));
            
            NodeTabContainer.OpenNode(vm);
            _currentFocusedVm = vm;

            // Trigger Navigated event so the UI updates
            InvokeFakeNavigated(typeof(DialogueNodePage), vm);
        }

        public override void CloseDialogueNode(DialogueNodeViewModel vm)
        {
            NodeTabContainer.CloseNode(vm);
        }

        public override object GoBackImplementation()
        {
            if (CanGoBack)
            {
                _currentFocusedVm = Frame.BackStack.Last().Parameter;

                // Nodes are a special case and use the tabcontainer instead of the frame
                if (_currentFocusedVm is DialogueNodeViewModel vm)
                {
                    NodeTabContainer.OpenNode(vm);
                    Frame.BackStack.Remove(Frame.BackStack.Last());
                    InvokeFakeNavigated(typeof(DialogueNodePage), vm);
                    return null;
                }
                else
                {
                    Frame.GoBack();
                    _lastParamUsed = _currentFocusedVm;
                    return _currentFocusedVm;
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
                    _frame = (Application.Current as App)?.Window.WindowContent as Frame;
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
