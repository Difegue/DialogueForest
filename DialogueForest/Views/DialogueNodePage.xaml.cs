using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Models;
using DialogueForest.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DialogueForest.Views
{
    public sealed partial class DialogueNodePage : Page
    {
        public DialogueNodeViewModel ViewModel => (DialogueNodeViewModel)DataContext;

        public DialogueNodePage()
        {
            InitializeComponent();

            DataContextChanged += (s,a) => Bindings.Update();
        }

    }

    /// <summary>
    /// Custom DataTemplate selector to show the correct UI depending on MetadataKind.
    /// </summary>
    public class MetadataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate MetadataBoolTemplate { get; set; }
        public DataTemplate MetadataStringTemplate { get; set; }
        public DataTemplate MetadataColorTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is MetadataViewModel vm)
            {
                switch (vm.Kind)
                {
                    case MetadataKind.BOOL: return MetadataBoolTemplate;
                    case MetadataKind.STRING: return MetadataStringTemplate;
                    case MetadataKind.COLOR: return MetadataColorTemplate;
                    default: return null;
                }
            }
            return null;
        }
    }
}
