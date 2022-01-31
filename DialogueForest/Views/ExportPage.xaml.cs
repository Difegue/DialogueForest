using System;

using DialogueForest.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace DialogueForest.Views
{
    public sealed partial class ExportPage : Page
    {
        //public ShellContentDialogViewModel ViewModel { get; } = new ShellContentDialogViewModel();

        private ExportPage(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            InitializeComponent();
            shellFrame.Navigate(pageType, parameter,  infoOverride);
            shellFrame.Width = Window.Current.Bounds.Width * 0.8;
            shellFrame.Height = Window.Current.Bounds.Height * 0.8;
        }

        public static ExportPage Create(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            return new ExportPage(pageType, parameter, infoOverride);
        }
    }
}
