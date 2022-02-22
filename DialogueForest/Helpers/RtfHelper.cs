using DialogueForest.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;

namespace DialogueForest.Helpers
{
    public static class RtfHelper
    {
        internal static string Convert(string rtf, OutputFormat format)
        {
            //TODO formats

            if (rtf == null) return "";

            var box = new RichEditBox();
            box.Document.SetText(TextSetOptions.FormatRtf, rtf);

            string text;
            box.Document.GetText(TextGetOptions.None, out text);

            return text;
        }
    }
}
