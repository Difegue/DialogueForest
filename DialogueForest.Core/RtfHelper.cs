using DialogueForest.Core.Interfaces;
using BracketPipe;
using RtfPipe;
using System.Text;

namespace DialogueForest.Core
{
    public static class RtfHelper
    {
        internal static string Convert(string rtf, OutputFormat format)
        {
            if (rtf == null) return "";

            // Handle codepages
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Use RtfPipe to have an HTML converted text as a base
            var settings = new RtfHtmlSettings
            {
                Indent = false,
                NewLineOnAttributes = false
            };
            var html = Rtf.ToHtml(rtf, settings);

            switch (format)
            {
                case OutputFormat.PlainText: return Html.ToPlainText(html);
                case OutputFormat.HTML: return html;
                case OutputFormat.BBCode: return Html.ToMarkdown(html);
                case OutputFormat.UnityRichText: return Html.ToMarkdown(html);
            }

            // dumb fail state
            return rtf; 
        }
    }
}
