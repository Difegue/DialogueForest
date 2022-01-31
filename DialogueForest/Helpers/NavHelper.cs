using System;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;

using Windows.UI.Xaml;

namespace DialogueForest.Helpers
{
    public class NavHelper
    {
        // This helper class allows to specify the page that will be shown when you click on a NavigationViewItem
        //
        // Usage in xaml:
        // <winui:NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:NavHelper.NavigateTo="views:MainPage" />
        //
        // Usage in code:
        // NavHelper.SetNavigateTo(navigationViewItem, typeof(MainPage));
        public static Type GetNavigateTo(NavigationViewItem item)
        {
            return (Type)item.GetValue(NavigateToProperty);
        }

        public static void SetNavigateTo(NavigationViewItem item, Type value)
        {
            item.SetValue(NavigateToProperty, value);
        }

        public static ICommand GetCommand(NavigationViewItem item)
        {
            return (ICommand)item.GetValue(CommandProperty);
        }

        public static void SetCommand(NavigationViewItem item, ICommand value)
        {
            item.SetValue(CommandProperty, value);
        }

        public static object GetCommandParameter(NavigationViewItem item)
        {
            return (object)item.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(NavigationViewItem item, object value)
        {
            item.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty NavigateToProperty =
            DependencyProperty.RegisterAttached("NavigateTo", typeof(Type), typeof(NavHelper), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(NavHelper), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(NavHelper), new PropertyMetadata(null));
    }
}
