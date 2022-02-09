﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;
using DialogueForest.ViewModels;

namespace DialogueForest.Core.ViewModels
{
    public  partial class TreeViewModelBase : ObservableObject
    {
        private IDialogService _dialogService;
        private IInteropService _interopService;
        private INavigationService _navigationService;
        private ForestDataService _dataService;

        protected DialogueTree _tree;

        public static TreeViewModelBase Create(DialogueTree tree)
        {
            var instance = Ioc.Default.GetRequiredService<TreeViewModelBase>();
            instance._tree = tree;

            return instance;
        }

        public ObservableCollection<DialogueNodeViewModel> Nodes { get; } = new ObservableCollection<DialogueNodeViewModel>();

        public TreeViewModelBase(IDialogService dialogService, IInteropService interopService, INavigationService navigationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _interopService = interopService;
            _dataService = forestService;

            Nodes.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsNodesEmpty));
            Nodes.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalDialogues));
        }

        public bool IsNodesEmpty => Nodes.Count == 0;

        public string Title
        {
            get => _tree.Name;
            set => SetProperty(_tree.Name, value, _tree, (u, n) => u.Name = n);
        }

        [ObservableProperty]
        private int _totalWords;

        public int TotalDialogues => Nodes.Count;

        [ICommand]
        private void ShowNode(DialogueNodeViewModel vm)
        {
            _navigationService.OpenDialogueNode(vm);
        }

        [ICommand]
        private void AddNode(DialogueNode node = null)
        {
            if (node == null)
            {
                node = _dataService.CreateNewNode();
            }
            _tree.AddNode(node);
            var nodeVm = DialogueNodeViewModel.Create(node);

            nodeVm.SetParentVm(this);
            Nodes.Add(nodeVm);
        }

        internal void MoveNodeToTrash(DialogueNodeViewModel nodeVm, DialogueNode node)
        {
            _dataService.MoveNodeToTrash(_tree, node);
            Nodes.Remove(nodeVm);
        }

        [ICommand]
        private void Delete()
        {

        }

        
    }
}
