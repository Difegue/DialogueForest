using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Text;

namespace DialogueForest.Controls
{
    public class BindableRichEditBox : RichEditBox
    {
        public static readonly DependencyProperty RtfTextProperty =
            DependencyProperty.Register(
            "RtfText", typeof(string), typeof(BindableRichEditBox),
            new PropertyMetadata(default(string), RtfTextPropertyChanged));

        public static readonly DependencyProperty PlainTextProperty =
            DependencyProperty.Register(
            "PlainText", typeof(string), typeof(BindableRichEditBox),
            new PropertyMetadata(default(string)));

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
                rtb.Document.SetText(TextSetOptions.FormatRtf, rtb.RtfText);
                rtb._lockChangeExecution = false;
            }
        }
    }
}
