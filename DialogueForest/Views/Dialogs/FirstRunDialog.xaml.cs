using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class FirstRunDialog : ContentDialog
    {
        public FirstRunDialog()
        {
            XamlRoot = (Application.Current as App)?.XamlRoot;
            RequestedTheme = ((Application.Current as App)?.Window.WindowContent as FrameworkElement).RequestedTheme;
            InitializeComponent();
        }
    }
}
