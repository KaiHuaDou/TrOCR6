using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace TrOCR.Ocr;

public class HtmlToText
{
    static HtmlToText( )
    {
        _tags.Add("address", "\n");
        _tags.Add("blockquote", "\n");
        _tags.Add("div", "\n");
        _tags.Add("dl", "\n");
        _tags.Add("fieldset", "\n");
        _tags.Add("form", "\n");
        _tags.Add("h1", "\n");
        _tags.Add("/h1", "\n");
        _tags.Add("h2", "\n");
        _tags.Add("/h2", "\n");
        _tags.Add("h3", "\n");
        _tags.Add("/h3", "\n");
        _tags.Add("h4", "\n");
        _tags.Add("/h4", "\n");
        _tags.Add("h5", "\n");
        _tags.Add("/h5", "\n");
        _tags.Add("h6", "\n");
        _tags.Add("/h6", "\n");
        _tags.Add("p", "\n");
        _tags.Add("/p", "\n");
        _tags.Add("table", "\n");
        _tags.Add("/table", "\n");
        _tags.Add("ul", "\n");
        _tags.Add("/ul", "\n");
        _tags.Add("ol", "\n");
        _tags.Add("/ol", "\n");
        _tags.Add("/li", "\n");
        _tags.Add("br", "\n");
        _tags.Add("/td", "\t");
        _tags.Add("/tr", "\n");
        _tags.Add("/pre", "\n");
        _ignoreTags = new HashSet<string>
            {
                "script",
                "noscript",
                "style",
                "object"
            };
    }

    public string Convert(string html)
    {
        _text = new TextBuilder( );
        _html = html;
        _pos = 0;
        while (!EndOfText)
        {
            if (Peek( ) == '<')
            {
                string text = ParseTag(out _);
                if (text == "body")
                    _text.Clear( );
                else if (text == "/body")
                {
                    _pos = _html.Length;
                }
                else if (text == "pre")
                {
                    _text.Preformatted = true;
                    EatWhitespaceToNextLine( );
                }
                else if (text == "/pre")
                {
                    _text.Preformatted = false;
                }
                if (_tags.TryGetValue(text, out string text2))
                    _text.Write(text2);
                if (_ignoreTags.Contains(text))
                    EatInnerContent(text);
            }
            else if (char.IsWhiteSpace(Peek( )))
            {
                _text.Write(_text.Preformatted ? Peek( ) : ' ');
                MoveAhead( );
            }
            else
            {
                _text.Write(Peek( ));
                MoveAhead( );
            }
        }
        return HttpUtility.HtmlDecode(_text.ToString( ));
    }

    protected string ParseTag(out bool selfClosing)
    {
        string text = string.Empty;
        selfClosing = false;
        if (Peek( ) == '<')
        {
            MoveAhead( );
            EatWhitespace( );
            int pos = _pos;
            if (Peek( ) == '/')
                MoveAhead( );
            while (!EndOfText && !char.IsWhiteSpace(Peek( )) && Peek( ) != '/' && Peek( ) != '>')
            {
                MoveAhead( );
            }
            text = _html.Substring(pos, _pos - pos).ToLower(System.Globalization.CultureInfo.CurrentCulture);
            while (!EndOfText && Peek( ) != '>')
            {
                if (Peek( ) is '"' or '\'')
                    EatQuotedValue( );
                else
                {
                    if (Peek( ) == '/')
                        selfClosing = true;
                    MoveAhead( );
                }
            }
            MoveAhead( );
        }
        return text;
    }

    protected void EatInnerContent(string tag)
    {
        string text = "/" + tag;
        while (!EndOfText)
        {
            if (Peek( ) == '<')
            {
                if (ParseTag(out bool flag) == text)
                    return;
                if (!flag && !tag.StartsWith("/"))
                    EatInnerContent(tag);
            }
            else
            {
                MoveAhead( );
            }
        }
    }

    protected bool EndOfText => _pos >= _html.Length;

    protected char Peek( ) => _pos >= _html.Length ? '\0' : _html[_pos];

    protected void MoveAhead( ) => _pos = Math.Min(_pos + 1, _html.Length);

    protected void EatWhitespace( )
    {
        while (char.IsWhiteSpace(Peek( )))
        {
            MoveAhead( );
        }
    }

    protected void EatWhitespaceToNextLine( )
    {
        while (char.IsWhiteSpace(Peek( )))
        {
            int num = Peek( );
            MoveAhead( );
            if (num == 10)
                break;
        }
    }

    protected void EatQuotedValue( )
    {
        char c = Peek( );
        if (c is '"' or '\'')
        {
            MoveAhead( );
            _pos = _html.IndexOfAny(new char[] { c, '\r', '\n' }, _pos);
            if (_pos < 0)
            {
                _pos = _html.Length;
                return;
            }
            MoveAhead( );
        }
    }

    protected static Dictionary<string, string> _tags = new( );

    protected static HashSet<string> _ignoreTags;

    protected TextBuilder _text;

    protected string _html;

    protected int _pos;

    protected class TextBuilder
    {
        public TextBuilder( )
        {
            _text = new StringBuilder( );
            _currLine = new StringBuilder( );
            _emptyLines = 0;
            _preformatted = false;
        }

        public bool Preformatted
        {
            get => _preformatted;
            set
            {
                if (value)
                {
                    if (_currLine.Length > 0)
                        FlushCurrLine( );
                    _emptyLines = 0;
                }
                _preformatted = value;
            }
        }

        public void Clear( )
        {
            _text.Length = 0;
            _currLine.Length = 0;
            _emptyLines = 0;
        }

        public void Write(string s)
        {
            foreach (char c in s)
            {
                Write(c);
            }
        }

        public void Write(char c)
        {
            if (_preformatted)
            {
                _text.Append(c);
                return;
            }
            if (c != '\r')
            {
                if (c == '\n')
                {
                    FlushCurrLine( );
                    return;
                }
                if (char.IsWhiteSpace(c))
                {
                    int length = _currLine.Length;
                    if (length == 0 || !char.IsWhiteSpace(_currLine[length - 1]))
                    {
                        _currLine.Append(' ');
                        return;
                    }
                }
                else
                {
                    _currLine.Append(c);
                }
            }
        }

        protected void FlushCurrLine( )
        {
            string text = _currLine.ToString( ).Trim( );
            if (text.Replace("\u00a0", string.Empty).Length == 0)
            {
                _emptyLines++;
                if (_emptyLines < 2 && _text.Length > 0)
                    _text.AppendLine(text);
            }
            else
            {
                _emptyLines = 0;
                _text.AppendLine(text);
            }
            _currLine.Length = 0;
        }

        public override string ToString( )
        {
            if (_currLine.Length > 0)
                FlushCurrLine( );
            return _text.ToString( );
        }

        private readonly StringBuilder _text;

        private readonly StringBuilder _currLine;

        private int _emptyLines;

        private bool _preformatted;
    }
}
