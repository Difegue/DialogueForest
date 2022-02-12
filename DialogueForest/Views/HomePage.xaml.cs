﻿using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DialogueForest.Views
{

    public sealed partial class HomePage : Page
    {
        public WelcomeViewModel ViewModel => (WelcomeViewModel)DataContext;

        public HomePage()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<WelcomeViewModel>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.UpdateWelcomeText();
        }
    }
}
