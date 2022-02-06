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
        private INotificationService _notificationService;
        private ForestDataService _dataService;

        private DialogueTree _tree;

        public ObservableCollection<DialogueNodeViewModel> Nodes { get; } = new ObservableCollection<DialogueNodeViewModel>();

        public TreeViewModelBase(IDialogService dialogService, IInteropService interopService, INotificationService notificationService, ForestDataService forestService)
        {
            _dialogService = dialogService;
            _notificationService = notificationService;
            _interopService = interopService;
            _dataService = forestService;

            Nodes.CollectionChanged += (s, e) => OnPropertyChanged(nameof(IsNodesEmpty));
        }

        public bool IsNodesEmpty => Nodes.Count == 0;

        [ObservableProperty]
        private string _title;

        [ICommand]
        private void AddNode()
        {
            // TODO: Link to forestservice data
            var nodeVm = Ioc.Default.GetRequiredService<DialogueNodeViewModel>();
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
