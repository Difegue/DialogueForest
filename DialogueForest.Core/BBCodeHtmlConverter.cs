using BracketPipe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

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

            var sb = new StringBuilder();
            sb.EnsureCapacity(html.Length);
            
            using (var sw = new StringWriter(sb))
            using (var reader = new HtmlReader(html, false))
            {
                using (var w = new BBCodeWriter(sw, null))
                {
                    Html.ToHtml(reader, w);
                    w.Flush();
                }
                return sw.ToString();
            }
        }
    }

    /// <summary>
    /// Builds a BBCode string from the HTMl written to the instance
    /// </summary>
    /// <seealso cref="System.Xml.XmlWriter" />
    public class BBCodeWriter : XmlWriter
    {
        private TextWriter _writer;
        private Stack<HtmlStartTag> _nodes = new Stack<HtmlStartTag>();
        private int _ignoreDepth = int.MaxValue;
        private int _boldDepth = int.MaxValue;
        private int _underlineDepth = int.MaxValue;
        private int _italicDepth = int.MaxValue;
        private StringBuilder _attributeBuffer;
        private string _currentAttribute;
        private InternalState _state = InternalState.Start;
        private MinifyState _minify = MinifyState.LastCharWasSpace;
        private MarkdownWriterSettings _settings;
        private List<string> _linePrefix = new List<string>();
        private PreserveState _preserveWhitespace = PreserveState.None;
        private bool _outputStarted;
        private List<HtmlStartTag> _colorTags = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="BBCodeWriter"/> class.
        /// </summary>
        /// <param name="writer">The writer to output markdown to.</param>
        public BBCodeWriter(TextWriter writer) : this(writer, new MarkdownWriterSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BBCodeWriter"/> class.
        /// </summary>
        /// <param name="writer">The writer to output markdown to.</param>
        /// <param name="settings">The settings.</param>
        public BBCodeWriter(TextWriter writer, MarkdownWriterSettings settings)
        {
            _writer = writer;
            _settings = settings ?? new MarkdownWriterSettings();
        }

        /// <summary>
        /// Gets the state of the writer.
        /// </summary>
        public override WriteState WriteState
        {
            get
            {
                return (WriteState)_state;
            }
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="ns">The namespace URI whose prefix you want to find.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override string LookupPrefix(string ns)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encodes the specified binary bytes as Base64 and writes out the resulting text.
        /// </summary>
        /// <param name="buffer">Byte array to encode.</param>
        /// <param name="index">The position in the buffer indicating the start of the bytes to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            WriteInternal(Convert.ToBase64String(buffer, index, count));
        }

        /// <summary>
        /// Writes out a block containing the specified text.
        /// </summary>
        /// <param name="text">The text to place inside the block.</param>
        public override void WriteCData(string text)
        {
            WriteInternal(text);
        }

        /// <summary>
        /// Forces the generation of a character entity for the specified Unicode character value.
        /// </summary>
        /// <param name="ch">The Unicode character for which to generate a character entity.</param>
        /// <exception cref="ArgumentException">Invalid surrogate: Missing low character</exception>
        public override void WriteCharEntity(char ch)
        {
            if (XmlCharType.IsSurrogate((int)ch))
                throw new ArgumentException("Invalid surrogate: Missing low character");

            switch (ch)
            {
                case '<':
                    WriteEntityRef("lt");
                    break;
                case '>':
                    WriteEntityRef("gt");
                    break;
                case '&':
                    WriteEntityRef("amp");
                    break;
                case '"':
                    WriteEntityRef("quot");
                    break;
                case '\'':
                    WriteEntityRef("apos");
                    break;
                default:
                    var num = (int)ch;
                    if (num == 160)
                    {
                        WriteEntityRef("nbsp");
                    }
                    else
                    {
                        var text = num.ToString("X", NumberFormatInfo.InvariantInfo);
                        WriteEntityRef("#x" + text);
                    }
                    break;
            }
        }

        /// <summary>
        /// Writes text one buffer at a time.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position in the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteChars(char[] buffer, int index, int count)
        {
            WriteInternal(new string(buffer, index, count));
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            // Do nothing
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="name">The name of the DOCTYPE. This must be non-empty.</param>
        /// <param name="pubid">If non-null it also writes PUBLIC "pubid" "sysid" where <paramref name="pubid" /> and <paramref name="sysid" /> are replaced with the value of the given arguments.</param>
        /// <param name="sysid">If <paramref name="pubid" /> is null and <paramref name="sysid" /> is non-null it writes SYSTEM "sysid" where <paramref name="sysid" /> is replaced with the value of this argument.</param>
        /// <param name="subset">If non-null it writes [subset] where subset is replaced with the value of this argument.</param>
        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            // Do nothing
        }

        /// <summary>
        /// Closes the previous <see cref="M:System.Xml.XmlWriter.WriteStartAttribute(System.String,System.String)" /> call.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override void WriteEndAttribute()
        {
            if (_nodes.Count < 1)
                throw new InvalidOperationException();
            
            var attributeValue = _attributeBuffer.ToString();
            _attributeBuffer = null;
            _nodes.Peek().Attributes.Add(new(_currentAttribute, attributeValue));

            if (_currentAttribute == "style" && attributeValue.Contains("color"))
            {
                var color = attributeValue.Substring(attributeValue.IndexOf("color", StringComparison.Ordinal) + 6);
                color = color.Substring(0, color.IndexOf(';'));

                _writer.Write("[color=");
                _writer.Write(color);
                _writer.Write("]");
                _colorTags.Add(_nodes.Peek());
            }
            
            _state = InternalState.Element;
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void WriteEndDocument()
        {
            // Do nothing
        }

        /// <summary>
        /// Closes one element and pops the corresponding namespace scope.
        /// </summary>
        public override void WriteEndElement()
        {
            WriteStartElementEnd();
            var start = _nodes.Pop();
            if (_ignoreDepth > _nodes.Count)
                _ignoreDepth = int.MaxValue;


            if (_colorTags.Contains(start))
            {
                _colorTags.Remove(start);
                _writer.Write("[/color]");
            }

            string buffer;
            switch (start.Value)
            {
                case "a":
                    if (start.Attributes.Count > 0)
                    {
                        _writer.Write("[/URL]");
                        _preserveWhitespace = PreserveState.None;
                    }
                    _minify = MinifyState.Compressed;
                    break;
                case "b":
                case "strong":
                    if (_boldDepth > _nodes.Count)
                    {
                        _writer.Write("[/b]");
                        _boldDepth = int.MaxValue;
                    }
                    break;
                case "u":
                    if (_underlineDepth > _nodes.Count)
                    {
                        _writer.Write("[/u]");
                        _boldDepth = int.MaxValue;
                    }
                    break;
                case "code":
                    if (_preserveWhitespace == PreserveState.None)
                        _writer.Write("[/code]");
                    break;
                case "em":
                case "i":
                    if (_italicDepth > _nodes.Count)
                    {
                        _writer.Write("[/i]");
                        _italicDepth = int.MaxValue;
                    }
                    break;
                
                case "h1":
                    _writer.Write("[/h1]");
                    break;
                case "h2":
                    _writer.Write("[/h2]");
                    break;
                case "h3":
                    _writer.Write("[/h3]");
                    break;
                case "h4":
                    _writer.Write("[/h4]");
                    break;
                case "h5":
                    _writer.Write("[/h5]");
                    break;
                case "h6":
                    _writer.Write("[/h6]");
                    break;
                case "p":
                case "blockquote":
                case "ol":
                case "ul":
                    EndBlock();
                    break;
                case "pre":
                    EndBlock();
                    _preserveWhitespace = PreserveState.None;
                    break;
            }
        }

        /// <summary>
        /// Writes out an entity reference as &amp;name;.
        /// </summary>
        /// <param name="name">The name of the entity reference.</param>
        public override void WriteEntityRef(string name)
        {
            WriteInternal("&");
            WriteInternal(name);
            WriteInternal(";");
        }

        /// <summary>
        /// Closes one element and pops the corresponding namespace scope.
        /// </summary>
        public override void WriteFullEndElement()
        {
            WriteEndElement();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="name">The name of the processing instruction.</param>
        /// <param name="text">The text to include in the processing instruction.</param>
        /// <exception cref="NotSupportedException"></exception>
        public override void WriteProcessingInstruction(string name, string text)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes raw markup manually from a string.
        /// </summary>
        /// <param name="data">String containing the text to write.</param>
        public override void WriteRaw(string data)
        {
            _writer.Write(data);
        }

        /// <summary>
        /// Writes raw markup manually from a character buffer.
        /// </summary>
        /// <param name="buffer">Character array containing the text to write.</param>
        /// <param name="index">The position within the buffer indicating the start of the text to write.</param>
        /// <param name="count">The number of characters to write.</param>
        public override void WriteRaw(char[] buffer, int index, int count)
        {
            _writer.Write(buffer, index, count);
        }

        /// <summary>
        /// Writes the start of an attribute with the specified prefix, local name, and namespace URI.
        /// </summary>
        /// <param name="prefix">The namespace prefix of the attribute.</param>
        /// <param name="localName">The local name of the attribute.</param>
        /// <param name="ns">The namespace URI for the attribute.</param>
        /// <exception cref="InvalidOperationException"></exception>
        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (_nodes.Count < 1)
                throw new InvalidOperationException();
            _currentAttribute = localName;
            _attributeBuffer = new();
            _state = InternalState.Attribute;
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void WriteStartDocument()
        {
            // Do nothing
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="standalone">If true, it writes "standalone=yes"; if false, it writes "standalone=no".</param>
        public override void WriteStartDocument(bool standalone)
        {
            // Do nothing
        }

        /// <summary>
        /// Writes the specified start tag and associates it with the given namespace and prefix.
        /// </summary>
        /// <param name="prefix">The namespace prefix of the element.</param>
        /// <param name="localName">The local name of the element.</param>
        /// <param name="ns">The namespace URI to associate with the element.</param>
        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            WriteStartElementEnd();
            var start = new HtmlStartTag(localName.ToLowerInvariant());
            _nodes.Push(start);
            
            switch (start.Value)
            {
                case "b":
                case "strong":
                    StartInline();
                    if (_boldDepth > _nodes.Count)
                    {
                        _boldDepth = _nodes.Count;
                        _writer.Write("[b]");
                    }
                    break;
                case "u":
                    StartInline();
                    if (_underlineDepth > _nodes.Count)
                    {
                        _underlineDepth = _nodes.Count;
                        _writer.Write("[u]");
                    }
                    break;
                case "blockquote":
                    StartBlock("> ");
                    break;
                case "br":
                    _minify = MinifyState.LastCharWasSpace;
                    switch (_preserveWhitespace)
                    {
                        case PreserveState.BeforeContent:
                            _preserveWhitespace = PreserveState.Preserve;
                            break;
                        case PreserveState.InternalLineFeed:
                            _writer.Write(_settings.NewLineChars);
                            WritePrefix();
                            _preserveWhitespace = PreserveState.InternalLineFeed;
                            break;
                        case PreserveState.None:
                            _writer.Write('\\');
                            _writer.Write(_settings.NewLineChars);
                            WritePrefix();
                            break;
                        default:
                            _preserveWhitespace = PreserveState.InternalLineFeed;
                            break;
                    }
                    break;
                case "code":
                    if (_preserveWhitespace == PreserveState.None)
                    {
                        StartInline();
                        _writer.Write("[code]");
                    }
                    break;
                case "em":
                case "i":
                    StartInline();
                    if (_italicDepth > _nodes.Count)
                    {
                        _italicDepth = _nodes.Count;
                        _writer.Write("[i]");
                    }
                    break;
                case "h1":
                    StartBlock("[h1]");
                    break;
                case "h2":
                    StartBlock("[h2]");
                    break;
                case "h3":
                    StartBlock("[h3]");
                    break;
                case "h4":
                    StartBlock("[h4]");
                    break;
                case "h5":
                    StartBlock("[h5]");
                    break;
                case "h6":
                    StartBlock("[h6]");
                    break;
                case "hr":
                    StartBlock("");
                    EndBlock();
                    _writer.Write(_settings.NewLineChars);
                    _minify = MinifyState.LastCharWasSpace;
                    break;
                case "li":
                    if (_outputStarted)
                        _writer.Write(_settings.NewLineChars);
                    if (_linePrefix.Count > 0
                      && char.IsDigit(_linePrefix[_linePrefix.Count - 1][0]))
                    {
                        var value = _linePrefix[_linePrefix.Count - 1];
                        value = string.Format("{0}. ", int.Parse(value.Substring(0, value.Length - 2)) + 1);
                        _linePrefix[_linePrefix.Count - 1] = value;
                    }
                    WritePrefix();
                    _minify = MinifyState.LastCharWasSpace;
                    break;
                case "p":
                    StartBlock("");
                    break;
                case "pre":
                    StartBlock("    ");
                    _preserveWhitespace = PreserveState.BeforeContent;
                    break;
                case "ol":
                    StartList("0. ");
                    break;
                case "ul":
                    StartList("- ");
                    break;
            }
            _state = InternalState.Element;
        }

        private void WritePrefix()
        {
            for (var i = 0; i < _linePrefix.Count; i++)
            {
                // Only indent on nested lists
                if (i < _linePrefix.Count - 1
                  && _linePrefix[i].Length > 0
                  && (_linePrefix[i][0] == '-' || char.IsDigit(_linePrefix[i][0])))
                    _writer.Write(new string(' ', _linePrefix[i].Length));
                else
                    _writer.Write(_linePrefix[i]);
            }
        }

        /// <summary>
        /// Writes the given text content.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public override void WriteString(string text)
        {
            WriteInternal(text);
        }

        /// <summary>
        /// Generates and writes the surrogate character entity for the surrogate character pair.
        /// </summary>
        /// <param name="lowChar">The low surrogate. This must be a value between 0xDC00 and 0xDFFF.</param>
        /// <param name="highChar">The high surrogate. This must be a value between 0xD800 and 0xDBFF.</param>
        /// <exception cref="InvalidOperationException">Invalid surrogate pair</exception>
        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            if (!XmlCharType.IsLowSurrogate((int)lowChar) || !XmlCharType.IsHighSurrogate((int)highChar))
                throw new InvalidOperationException("Invalid surrogate pair");

            var num = XmlCharType.CombineSurrogateChar((int)lowChar, (int)highChar);
            WriteInternal("&#x");
            WriteInternal(num.ToString("X", NumberFormatInfo.InvariantInfo));
            WriteInternal(";");
        }

        /// <summary>
        /// Writes out the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public override void WriteWhitespace(string ws)
        {
            WriteInternal(ws);
        }

        private void WriteInternal(string value)
        {
            WriteStartElementEnd();

            if (_ignoreDepth <= _nodes.Count)
                return;

            if (_preserveWhitespace == PreserveState.HtmlEncoding)
            {
                WriteEscaped(value);
                _outputStarted = true;
                return;
            }

            if (_attributeBuffer != null)
            {
                _attributeBuffer.Append(value);
            }
            else
            {
                for (var i = 0; i < value.Length; i++)
                {
                    if (_preserveWhitespace == PreserveState.InternalLineFeed)
                    {
                        _writer.Write(_settings.NewLineChars);
                        WritePrefix();
                        _preserveWhitespace = PreserveState.Preserve;
                    }

                    if (char.IsWhiteSpace(value[i]) && value[i] != '\u00A0')
                    {
                        if (_preserveWhitespace == PreserveState.BeforeContent
                            && value[i] == (Char)0x0a) // LF
                        {
                            _preserveWhitespace = PreserveState.Preserve;
                        }
                        else if (_preserveWhitespace != PreserveState.None)
                        {
                            if (value[i] == (Char)0x0a) // LF
                            {
                                _preserveWhitespace = PreserveState.InternalLineFeed;
                            }
                            else
                            {
                                _writer.Write(value[i]);
                            }
                        }
                        else
                        {
                            if (_minify != MinifyState.LastCharWasSpace
                                && _minify != MinifyState.BlockEnd)
                                _minify = MinifyState.SpaceNeeded;
                        }
                    }
                    else
                    {
                        switch (_minify)
                        {
                            case MinifyState.BlockEnd:
                                _writer.Write(_settings.NewLineChars);
                                _writer.Write(_settings.NewLineChars);
                                _minify = MinifyState.Compressed;
                                break;
                            case MinifyState.SpaceNeeded:
                                _writer.Write(' ');
                                _minify = MinifyState.Compressed;
                                break;
                            case MinifyState.LastCharWasSpace:
                                _minify = MinifyState.Compressed;
                                break;
                        }

                        if (_preserveWhitespace == PreserveState.None)
                        {
                            switch (value[i])
                            {
                                case '\\':
                                case ']':
                                case '[':
                                    _writer.Write('\\');
                                    break;
                            }
                        }
                        else if (_preserveWhitespace == PreserveState.BeforeContent)
                        {
                            _preserveWhitespace = PreserveState.Preserve;
                        }
                        _writer.Write(value[i]);
                    }
                }
                _outputStarted = true;
            }
        }

        private void WriteStartElementEnd()
        {
            if (_nodes.Count > 0 && _state == InternalState.Element)
            {
                var start = _nodes.Peek();
                
                string buffer;
                switch (start.Value)
                {
                    case "a":
                        buffer = null;
                        StartInline();
                        if (start.Attributes.Count > 0
                          && (!start.TryGetValue("href", out buffer)
                            || string.IsNullOrEmpty(buffer)))
                        {
                            _writer.Write("[url= ");
                            foreach (var attr in start)
                            {
                                if (!string.IsNullOrEmpty(attr.Value))
                                {
                                    _writer.Write('"');
                                    WriteEscaped(attr.Value);
                                    _writer.Write('"');
                                }
                            }
                            _writer.Write(']');
                            _preserveWhitespace = PreserveState.HtmlEncoding;
                        }
                        break;
                    case "img":
                        StartInline();
                        _writer.Write("[img");
                        if (start.TryGetValue("title", out buffer))
                        {
                            _writer.Write('=');
                            _writer.Write('"');
                            _writer.Write(buffer);
                            _writer.Write('"');
                        }
                        _writer.Write(']');
                        _minify = MinifyState.Compressed;
                        break;
                    default:
                        break;
                }
                _state = InternalState.Content;
            }
        }

        private void StartInline()
        {
            if (_minify == MinifyState.SpaceNeeded)
            {
                _writer.Write(' ');
                _minify = MinifyState.LastCharWasSpace;
            }
        }

        private void StartList(string prefix)
        {
            if (_minify == MinifyState.Compressed
              || _minify == MinifyState.SpaceNeeded
              || _minify == MinifyState.BlockEnd)
            {
                _writer.Write(_settings.NewLineChars);
                _minify = MinifyState.LastCharWasSpace;
            }
            _linePrefix.Add(prefix ?? string.Empty);
        }

        private void StartBlock(string prefix)
        {
            var prefixRequired = false;
            if (_minify == MinifyState.Compressed
              || _minify == MinifyState.SpaceNeeded
              || _minify == MinifyState.BlockEnd)
            {
                _writer.Write(_settings.NewLineChars);
                _writer.Write(_settings.NewLineChars);
                _minify = MinifyState.LastCharWasSpace;
                prefixRequired = true;
            }
            _linePrefix.Add(prefix ?? string.Empty);
            if (prefixRequired || prefix != string.Empty)
                WritePrefix();
            _outputStarted = true;
        }

        private void EndBlock()
        {
            _minify = MinifyState.BlockEnd;
            if (_linePrefix.Count > 0)
                _linePrefix.RemoveAt(_linePrefix.Count - 1);
        }

        private void WriteEscaped(string text)
        {
            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case (Char)0x26: _writer.Write("&amp;"); break;
                    case (Char)0xa0: _writer.Write("&nbsp;"); break;
                    case (Char)0x3e: _writer.Write("&gt;"); break;
                    case (Char)0x3c: _writer.Write("&lt;"); break;
                    case (Char)0x22:
                        if (_settings.QuoteChar == (Char)0x22)
                            _writer.Write("&quot;");
                        else
                            _writer.Write('"');
                        break;
                    case (Char)0x27:
                        if (_settings.QuoteChar == (Char)0x27)
                            _writer.Write("&apos;");
                        else
                            _writer.Write("'");
                        break;
                    case (Char)0x0a: _writer.Write(_settings.NewLineChars); break;
                    default: _writer.Write(text[i]); break;
                }
            }
        }


#if !PORTABLE
        public override void Close()
        {

        }
#endif

        private enum InternalState
        {
            Start = 0,
            Element = 2,
            Attribute = 3,
            Content = 4,
        }

        private enum PreserveState : byte
        {
            None,
            HtmlEncoding,
            BeforeContent,
            Preserve,
            InternalLineFeed
        }

        private enum MinifyState : byte
        {
            Compressed,
            LastCharWasSpace,
            SpaceNeeded,
            InlineStartAfterSpace,
            BlockEnd
        }

        internal static class XmlCharType
        {
            internal static int CombineSurrogateChar(int lowChar, int highChar)
            {
                return lowChar - 56320 | (highChar - 55296 << 10) + 65536;
            }

            internal static bool IsHighSurrogate(int ch)
            {
                return XmlCharType.InRange(ch, 55296, 56319);
            }

            internal static bool IsLowSurrogate(int ch)
            {
                return XmlCharType.InRange(ch, 56320, 57343);
            }

            internal static bool IsSurrogate(int ch)
            {
                return XmlCharType.InRange(ch, 55296, 57343);
            }

            private static bool InRange(int value, int start, int end)
            {
                return value >= start && value <= end;
            }
        }
    }
}
