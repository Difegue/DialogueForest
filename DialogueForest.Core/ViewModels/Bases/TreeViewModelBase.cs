using System;
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

        private DialogueTree _tree;

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

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private int _totalWords;

        public int TotalDialogues => Nodes.Count;

        [ICommand]
        private void ShowNode(DialogueNodeViewModel vm)
        {
            _navigationService.OpenDialogueNode(vm);
        }

        [ICommand]
        private void AddNode()
        {
            // TODO: Link to forestservice data
            var nodeVm = Ioc.Default.GetRequiredService<DialogueNodeViewModel>();
            nodeVm.NodeTitle = "My New Dialogue";

            AddExistingNode(nodeVm);
        }

        private void AddExistingNode(DialogueNodeViewModel nodeVm)
        {
            nodeVm.SetParentVm(this);
            Nodes.Add(nodeVm);
        }

        public void RemoveNode(DialogueNodeViewModel vm) => Nodes.Remove(vm);

        [ICommand]
        private void Delete()
        {

        }

    }
}
