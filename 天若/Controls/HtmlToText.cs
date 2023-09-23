using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace TrOCR.Controls;

public class HtmlToText
{
    public string Convert(string html)
    {
        text = new TextBuilder( );
        this.html = html;
        position = 0;
        while (!EndOfText)
        {
            if (Peek( ) == '<')
            {
                string text = ParseTag(out _);
                switch (text)
                {
                    case "body": this.text.Clear( ); break;
                    case "/body": position = this.html.Length; break;
                    case "pre": this.text.Preformatted = true; EatWhitespaceToNextLine( ); break;
                    case "/pre": this.text.Preformatted = false; break;
                }
                if (tags.TryGetValue(text, out string text2))
                    this.text.Write(text2);
                if (ignoreTags.Contains(text))
                    EatInnerContent(text);
            }
            else if (char.IsWhiteSpace(Peek( )))
            {
                text.Write(text.Preformatted ? Peek( ) : ' ');
                MoveAhead( );
            }
            else
            {
                text.Write(Peek( ));
                MoveAhead( );
            }
        }
        return HttpUtility.HtmlDecode(text.ToString( ));
    }

    protected string ParseTag(out bool selfClosing)
    {
        string text = string.Empty;
        selfClosing = false;
        if (Peek( ) == '<')
        {
            MoveAhead( );
            EatWhitespace( );
            int pos = position;
            if (Peek( ) == '/')
                MoveAhead( );
            while (!EndOfText && !char.IsWhiteSpace(Peek( )) && Peek( ) != '/' && Peek( ) != '>')
            {
                MoveAhead( );
            }
            text = html.Substring(pos, position - pos).ToLower(System.Globalization.CultureInfo.CurrentCulture);
            while (!EndOfText && Peek( ) != '>')
            {
                if (Peek( ) is '"' or '\'')
                {
                    EatQuotedValue( );
                }
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

    protected bool EndOfText
        => position >= html.Length;

    protected char Peek( )
        => position >= html.Length ? '\0' : html[position];

    protected void MoveAhead( )
        => position = Math.Min(position + 1, html.Length);

    protected void EatWhitespace( )
    {
        while (char.IsWhiteSpace(Peek( )))
            MoveAhead( );
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
            position = html.IndexOfAny(new char[] { c, '\r', '\n' }, position);
            if (position < 0)
            {
                position = html.Length;
                return;
            }
            MoveAhead( );
        }
    }

    private static Dictionary<string, string> tags = new( )
    {
        {"address", "\n"},
        {"blockquote", "\n"},
        {"div", "\n"},
        {"dl", "\n"},
        {"fieldset", "\n"},
        {"form", "\n"},
        {"h1", "\n"},
        {"/h1", "\n"},
        {"h2", "\n"},
        {"/h2", "\n"},
        {"h3", "\n"},
        {"/h3", "\n"},
        {"h4", "\n"},
        {"/h4", "\n"},
        {"h5", "\n"},
        {"/h5", "\n"},
        {"h6", "\n"},
        {"/h6", "\n"},
        {"p", "\n"},
        {"/p", "\n"},
        {"table", "\n"},
        {"/table", "\n"},
        {"ul", "\n"},
        {"/ul", "\n"},
        {"ol", "\n"},
        {"/ol", "\n"},
        {"/li", "\n"},
        {"br", "\n"},
        {"/td", "\t"},
        {"/tr", "\n"},
        { "/pre", "\n"},
    };

    private static HashSet<string> ignoreTags =
        new( )
        {
            "script",
            "noscript",
            "style",
            "object"
        };
    private TextBuilder text;
    private string html;
    private int position;

    protected class TextBuilder
    {
        private readonly StringBuilder text;
        private readonly StringBuilder curLine;
        private int emptyLines;
        private bool preformatted;

        public TextBuilder( )
        {
            text = new StringBuilder( );
            curLine = new StringBuilder( );
            emptyLines = 0;
            preformatted = false;
        }

        public bool Preformatted
        {
            get => preformatted;
            set
            {
                if (value)
                {
                    if (curLine.Length > 0)
                        FlushCurrLine( );
                    emptyLines = 0;
                }
                preformatted = value;
            }
        }

        public void Clear( )
        {
            text.Length = 0;
            curLine.Length = 0;
            emptyLines = 0;
        }

        public void Write(string s)
        {
            foreach (char c in s)
            {
                Write(c);
            }
        }

        public void Write(char ch)
        {
            if (preformatted)
            {
                text.Append(ch);
                return;
            }
            if (ch != '\r')
            {
                if (ch == '\n')
                {
                    FlushCurrLine( );
                    return;
                }
                if (char.IsWhiteSpace(ch))
                {
                    int length = curLine.Length;
                    if (length == 0 || !char.IsWhiteSpace(curLine[length - 1]))
                    {
                        curLine.Append(' ');
                        return;
                    }
                }
                else
                {
                    curLine.Append(ch);
                }
            }
        }

        protected void FlushCurrLine( )
        {
            string text = curLine.ToString( ).Trim( );
            if (text.Replace("\u00a0", string.Empty).Length == 0)
            {
                emptyLines++;
                if (emptyLines < 2 && this.text.Length > 0)
                    this.text.AppendLine(text);
            }
            else
            {
                emptyLines = 0;
                this.text.AppendLine(text);
            }
            curLine.Length = 0;
        }

        public override string ToString( )
        {
            if (curLine.Length > 0)
                FlushCurrLine( );
            return text.ToString( );
        }
    }
}
