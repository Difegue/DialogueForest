using BracketPipe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using RtfPipe.Tokens;
using RtfPipe;
using RtfPipe.Model;
using System.Text.RegularExpressions;

namespace DialogueForest.Core
{
    
    /// <summary>
    /// Bare-bones BBCode output from a given HTML string.
    /// </summary>
    public static class BBCodeHtmlConverter
    {
        /// <summary>
        /// Convert RtfPipe HTML to BBCode for Unity/MarkLight.
        /// Use basic replaces to strip HTML away and insert BBCode tags instead.
        /// This is enough for our usecase, considering RtfPipe's HTML output is normalized.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string Convert(string html)
        {

            // Regex first for colors (only tag we handle that needs that kind of precision)
            var pattern = @"<p.*color:([^;]*);.*"">(.*?)<\/p>";
            html = Regex.Replace(html, pattern, m => $"[color={m.Groups[1].Value}]{m.Groups[2].Value}[/color]\n");
            
            html = Regex.Replace(html, @"<p style=""[^""]*"">", "");
            html = Regex.Replace(html, @"<div style=""[^""]*"">", "");
            html = Regex.Replace(html, @"<span style=""[^""]*"">", "");
            html = Regex.Replace(html, @"<em style=""[^""]*"">", "[i]");
            html = Regex.Replace(html, @"<strong style=""[^""]*"">", "[b]");
            html = Regex.Replace(html, @"<u style=""[^""]*"">", "[u]");

            html = html.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&nbsp;", "")
                       .Replace("<u>", "[u]").Replace("</u>", "[/u]").Replace("<strong>", "[b]").Replace("</strong>", "[/b]")
                       .Replace("<em>", "[i]").Replace("</em>", "[/i]")
                       .Replace("<br>", "").Replace("</div>", "").Replace("</span>", "").Replace("</p>", "\n");



            return html;
        }
    }
}
