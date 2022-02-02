﻿using DialogueForest.ViewModels;
using DialogueForest.Views;
using Microsoft.Toolkit.Uwp.UI.Animations;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DialogueForest.Services
{
    public class NavigationService : NavigationServiceBase
    {
        private Dictionary<Type, Type> _viewModelToPageDictionary = new Dictionary<Type, Type>()
        {
            { typeof(DialogueNodeViewModel), typeof(DialogueNodePage) },
            { typeof(SettingsViewModel), typeof(SettingsPage) },
            { typeof(GraphViewViewModel), typeof(GraphViewPage) },

            { typeof(DialogueTreeViewModel), typeof(DialogueTreePage) },
            { typeof(NotesViewModel), typeof(DialogueTreePage) },
            { typeof(PinsViewModel), typeof(DialogueTreePage) },
            { typeof(TrashViewModel), typeof(DialogueTreePage) },

            { typeof(ExportViewModel), typeof(ExportPage) }
        };

        public NavigationService()
        {

        }

        public override bool CanGoBack => Frame.CanGoBack;

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

        public override bool GoBackImplementation()
        {
            if (CanGoBack)
            {
                Frame.GoBack();
                return true;
            }

            return false;
        }

        private Frame _frame;
        public Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as Frame;
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
