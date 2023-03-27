using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.ViewModels;
using DialogueForest.ViewModels;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace DialogueForest.Views
{
    public sealed partial class ExportPage : Page
    {
        public ExportViewModel ViewModel => (ExportViewModel)DataContext;

        public ExportPage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<ExportViewModel>();
        }

    }
}
