using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Controls;
using DialogueForest.Core.Models;
using DialogueForest.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Text;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace DialogueForest.Views
{
    public sealed partial class DialogueNodePage : Page
    {
        public DialogueNodeViewModel ViewModel => DataContext is DialogueNodeViewModel vm ? vm : null; 

        public DialogueNodePage()
        {
            InitializeComponent();

            DataContextChanged += (s,a) => Bindings.Update();
        }

        private void Toggle_Bold(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToggleButton;
            var editor = tb.Tag as BindableRichEditBox;

            editor.Document.Selection.CharacterFormat.Bold = FormatEffect.Toggle;
            editor.Focus(FocusState.Programmatic);
        }

        private void Toggle_Italic(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToggleButton;
            var editor = tb.Tag as BindableRichEditBox;

            editor.Document.Selection.CharacterFormat.Italic = FormatEffect.Toggle;
            editor.Focus(FocusState.Programmatic);
        }

        private void Toggle_Underline(object sender, RoutedEventArgs e)
        {
            var tb = sender as ToggleButton;
            var editor = tb.Tag as BindableRichEditBox;

            editor.Document.Selection.CharacterFormat.Underline = tb.IsChecked.GetValueOrDefault() ? UnderlineType.Single : UnderlineType.None;
            editor.Focus(FocusState.Programmatic);
        }

        private void Selection_Changed(object sender, RoutedEventArgs e)
        {
            var editor = sender as BindableRichEditBox;
            var selection = editor.Document.Selection;
            var controls = editor.Tag as StackPanel;

            var boldBtn = controls.Children[0] as ToggleButton;
            var italicBtn = controls.Children[1] as ToggleButton;
            var underlineBtn = controls.Children[2] as ToggleButton;
            var colorBtn = controls.Children[4] as ColorPickerButton;

            boldBtn.IsChecked = selection.CharacterFormat.Bold == FormatEffect.On;
            italicBtn.IsChecked = selection.CharacterFormat.Italic == FormatEffect.On;
            underlineBtn.IsChecked = selection.CharacterFormat.Underline != UnderlineType.None;

            colorBtn.SelectedColor = selection.CharacterFormat.ForegroundColor;
            colorBtn.ColorPicker.Color = selection.CharacterFormat.ForegroundColor;
        }

        private void ColorPicker_Click(object sender, RoutedEventArgs e)
        {
            var cpb = sender as ColorPickerButton;
            var editor = cpb.Tag as BindableRichEditBox;

            // TODO: Update the toolkit colorpickerbutton so it exposes ColorChanged, this is atrocious and leaks eventlisteners
            cpb.ColorPicker.Tag = editor;
            cpb.ColorPicker.ColorChanged += ColorFlyout_ColorChanged;
            cpb.Flyout.Closing += (s,e) => cpb.ColorPicker.ColorChanged -= ColorFlyout_ColorChanged;
        }

        private void ColorFlyout_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs e)
        {
            var editor = sender.Tag as BindableRichEditBox;
            editor.Document.Selection.CharacterFormat.ForegroundColor = e.NewColor;
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
