using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Text;
using DialogueForest.Core;
using System;
using System.Linq;

namespace DialogueForest.Controls
{
    public partial class BindableRichEditBox : RichEditBox
    {
        public static readonly DependencyProperty RtfTextProperty =
            DependencyProperty.Register(
            "RtfText", typeof(string), typeof(BindableRichEditBox),
            new PropertyMetadata(default(string), RtfTextPropertyChanged));

        public static readonly DependencyProperty PlainTextProperty =
            DependencyProperty.Register(
            "PlainText", typeof(string), typeof(BindableRichEditBox),
            new PropertyMetadata(default(string)));

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
            "IsFocused", typeof(bool), typeof(BindableRichEditBox),
            new PropertyMetadata(default(bool), IsFocusedPropertyChanged));

        private bool _lockChangeExecution;

        public BindableRichEditBox()
        {
            TextChanged += BindableRichEditBox_TextChanged;
        }

        public string RtfText
        {
            get { return (string)GetValue(RtfTextProperty); }
            set { SetValue(RtfTextProperty, value); }
        }

        /// <summary>
        /// Read-only Plaintext output.
        /// </summary>
        public string PlainText
        {
            get { return (string)GetValue(PlainTextProperty); }
            set { SetValue(PlainTextProperty, value); }
        }

        public bool IsFocused
        {
            get { return (bool)GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }

        private void BindableRichEditBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!_lockChangeExecution)
            {
                _lockChangeExecution = true;
                string plainText;
                Document.GetText(TextGetOptions.None, out plainText);
                if (string.IsNullOrWhiteSpace(plainText))
                {
                    RtfText = "";
                    PlainText = "";
                }
                else
                {
                    Document.GetText(TextGetOptions.FormatRtf, out string text);
                    RtfText = text;
                    PlainText = plainText;
                }
                _lockChangeExecution = false;
            }
        }

        private static void RtfTextPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rtb = dependencyObject as BindableRichEditBox;
            if (rtb == null) return;
            if (!rtb._lockChangeExecution)
            {
                rtb._lockChangeExecution = true;

                // https://github.com/microsoft/microsoft-ui-xaml/issues/1941 -- needs the extra defaults option to avoid adding extraneous newlines..
                var options = TextSetOptions.FormatRtf | TextSetOptions.ApplyRtfDocumentDefaults;

                // Make black text white if dark theme is requested
                var text = (Application.Current as App)?.Window.WindowContent is FrameworkElement fe
                    ? fe.ActualTheme == ElementTheme.Light
                        ? rtb.RtfText.ConvertRtfWhiteTextToBlack()
                        : rtb.RtfText.ConvertRtfBlackTextToWhite()
                    : rtb.RtfText.ConvertRtfWhiteTextToBlack(); // Assume light theme if actual theme cannot be determined

                rtb.Document.SetText(options, text);
                rtb._lockChangeExecution = false;
            }

            // HACK: RichEditBox doesn't like having its text set programmatically and won't update its height,
            // so we give it a helpful nudge by setting a minheight fitting the amount of linebreaks in the text.
            rtb.Document.GetText(TextGetOptions.UseLf, out var plainTxt);
            rtb.MinHeight = 16 + plainTxt.Split("\n").Count() * 16;
        }

        private static void IsFocusedPropertyChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var rtb = dependencyObject as BindableRichEditBox;
            if (rtb == null) return;
            if (rtb.IsFocused)
                rtb.Focus(FocusState.Programmatic);
        }
    }
}
