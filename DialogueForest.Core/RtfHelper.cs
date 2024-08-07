﻿using DialogueForest.Core.Interfaces;
using BracketPipe;
using RtfPipe;
using System.Text;

namespace DialogueForest.Core
{
    public class CnEncodingProvider : EncodingProvider
    {
        public override Encoding GetEncoding(int codepage) => null;

        // Fix for this encoding being dropped from modern .NET but still used in BracketPipe 
        public override Encoding GetEncoding(string name)
        {
            if (name == "iso-2022-cn") return Encoding.GetEncoding("x-cp50227");
            return null;
        }
    }

    public static class RtfHelper
    {
        public static string ConvertRtfWhiteTextToBlack(this string s)
        => s?.Replace("\\red255\\green255\\blue255", "\\red0\\green0\\blue0");

        public static string ConvertRtfBlackTextToWhite(this string s)
            => s?.Replace("\\red0\\green0\\blue0", "\\red255\\green255\\blue255");

        public static string ConvertRtfToPlainText(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding.RegisterProvider(new CnEncodingProvider());

            // Use RtfPipe to have an HTML converted text as a base
            var settings = new RtfHtmlSettings
            {
                Indent = false,
                NewLineOnAttributes = false
            };
            var html = Rtf.ToHtml(s, settings);

            return Html.ToPlainText(html);
        }

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

            // Remove pure B/W color tags which can be inserted by app theme
            html = html.Replace("color:#FFFFFF;", "").Replace("color:#000000;", "");

            switch (format)
            {
                case OutputFormat.PlainText: return Html.ToPlainText(html);
                case OutputFormat.HTML: return html;
                case OutputFormat.BBCode: return BBCodeHtmlConverter.Convert(html);
                case OutputFormat.Markdown: return Html.ToMarkdown(html);
            }

            // dumb fail state
            return rtf; 
        }
    }
}
