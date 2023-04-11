using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Core.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using DialogueForest.ViewModels;
using Microsoft.UI.Xaml;
using CommunityToolkit.WinUI.UI.Controls;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;

namespace DialogueForest.Views
{
    public sealed partial class PinsPage : Page
    {
        public PinnedNodesViewModel ViewModel => (PinnedNodesViewModel)DataContext;

        public PinsPage()
        {
            InitializeComponent();
            DataContext = ((App)Application.Current).Services.GetService(typeof(PinnedNodesViewModel));
        }

        private void dg_Sorting(object sender, DataGridColumnEventArgs e)
        {
            if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
            {
                ViewModel.SortPins("Asc", e.Column.Tag.ToString());
                e.Column.SortDirection = DataGridSortDirection.Ascending;
            }
            else
            {
                ViewModel.SortPins("Desc", e.Column.Tag.ToString());
                e.Column.SortDirection = DataGridSortDirection.Descending;
            }
            
            // Remove sorting indicators from other columns
            foreach (var dgColumn in dataGrid.Columns)
            {
                if (dgColumn.Tag.ToString() != e.Column.Tag.ToString())
                {
                    dgColumn.SortDirection = null;
                }
            }
        }
    }
}
