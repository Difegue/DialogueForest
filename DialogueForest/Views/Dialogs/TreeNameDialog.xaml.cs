using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


namespace DialogueForest.Views
{

    public sealed partial class TreeNameDialog : ContentDialog
    {
        public TreeNameDialog()
        {
            this.InitializeComponent();
            XamlRoot = (Application.Current as App)?.XamlRoot;
            //TreeName = Localization.Strings.Resources.TreeNameDialogDefaultName;
        }

        public string TreeName { get; internal set; }
    }
}
