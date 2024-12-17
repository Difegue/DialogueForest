using System;
using DialogueForest.Controls;
using DialogueForest.Core.Models;
using DialogueForest.Core.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Text;
using CommunityToolkit.WinUI.UI.Controls;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml.Input;

namespace DialogueForest.Views
{
    public sealed partial class DialogueNodePage : Page
    {
        public DialogueNodeViewModel ViewModel => DataContext is DialogueNodeViewModel vm ? vm : null; 

        public DialogueNodePage()
        {
            InitializeComponent();

            DataContextChanged += (s,a) => Bindings.Update();

            // https://stackoverflow.com/questions/50242909/what-event-is-triggered-when-the-mouse-pointer-enters-a-menuflyoutsubitem-elemen
            LinkedBySubItem.AddHandler(PointerEnteredEvent, new PointerEventHandler(GetLinkedNodes), true);
            LinksToSubItem.AddHandler(PointerEnteredEvent, new PointerEventHandler(GetLinkedNodes), true);
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

            // This is totally going to break at some point.
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

        private void ComboBox_DragOver(object sender, DragEventArgs e)
        {
            // Only accept text (aka Dialogue Node IDs)
            e.AcceptedOperation = (e.DataView.Contains(StandardDataFormats.Text)) ? DataPackageOperation.Link : DataPackageOperation.None;
        }

        private async void ComboBox_Drop(object sender, DragEventArgs e)
        {
            // This test is in theory not needed as we returned DataPackageOperation.None if
            // the DataPackage did not contained text. However, it is always better if each
            // method is robust by itself
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                // We need to take a Deferral as we won't be able to confirm the end
                // of the operation synchronously
                var def = e.GetDeferral();
                var text = await e.DataView.GetTextAsync();

                // If this is an ID, assign it to SelectedItem - VM bindings will handle the rest
                if (long.TryParse(text, out var nodeId))
                    ((ComboBox)sender).SelectedItem = nodeId;

                e.AcceptedOperation = DataPackageOperation.Link;
                def.Complete();
            }
        }

        private void GetLinkedNodes(object sender, PointerRoutedEventArgs e) => Helpers.UWPHelpers.LoadLinkedNodesIntoMenuFlyout(sender as MenuFlyoutSubItem, ViewModel);
    }

    /// <summary>
    /// Custom DataTemplate selector to show the correct UI depending on MetadataKind.
    /// </summary>
    public partial class MetadataTemplateSelector : DataTemplateSelector
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
