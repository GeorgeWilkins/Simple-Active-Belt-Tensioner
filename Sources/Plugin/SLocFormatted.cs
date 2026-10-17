using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    public class SLocFormatted : Span
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            nameof(Key),
            typeof(string),
            typeof(SLocFormatted),
            new PropertyMetadata(null, OnKeyChanged)
        );

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public SLocFormatted()
        {
            Loaded += (_, __) => Refresh();
        }

        private static void OnKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SLocFormatted)d).Refresh();
        }

        private void Refresh()
        {
            Inlines.Clear();

            string key = Key;
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            string value = SLoc.GetValue(key) ?? string.Empty;
            InlineParser.AppendFormatted(Inlines, value);
        }

        private static class InlineParser
        {
            private const string HttpsPrefix = "https://";

            public static void AppendFormatted(InlineCollection inlines, string text)
            {
                if (inlines == null || string.IsNullOrEmpty(text))
                {
                    return;
                }

                var plain = new StringBuilder();

                for (int index = 0; index < text.Length; index++)
                {
                    char current = text[index];

                    if (current == '\r' || current == '\n')
                    {
                        FlushPlain(inlines, plain);
                        inlines.Add(new LineBreak());

                        if (current == '\r' && index + 1 < text.Length && text[index + 1] == '\n')
                        {
                            index++;
                        }

                        continue;
                    }

                    if (TryReadUrl(text, index, out var url, out var consumedLength))
                    {
                        FlushPlain(inlines, plain);

                        var link = new Hyperlink(new Run(url))
                        {
                            NavigateUri = new Uri(url, UriKind.Absolute),
                            ToolTip = url
                        };
                        link.RequestNavigate += OnLinkRequestNavigate;

                        inlines.Add(link);
                        index += consumedLength - 1;
                        continue;
                    }

                    if (current == '*' && index + 1 < text.Length && text[index + 1] == '*')
                    {
                        int closeIndex = text.IndexOf("**", index + 2, StringComparison.Ordinal);
                        if (closeIndex > index + 2)
                        {
                            FlushPlain(inlines, plain);

                            var bold = new Bold();
                            AppendFormatted(bold.Inlines, text.Substring(index + 2, closeIndex - (index + 2)));
                            inlines.Add(bold);

                            index = closeIndex + 1;
                            continue;
                        }
                    }

                    if (current == '_' || current == '*')
                    {
                        int closeIndex = text.IndexOf(current, index + 1);
                        if (closeIndex > index + 1)
                        {
                            FlushPlain(inlines, plain);

                            var italic = new Italic();
                            AppendFormatted(italic.Inlines, text.Substring(index + 1, closeIndex - (index + 1)));
                            inlines.Add(italic);

                            index = closeIndex;
                            continue;
                        }
                    }

                    plain.Append(current);
                }

                FlushPlain(inlines, plain);
            }

            private static bool TryReadUrl(string text, int startIndex, out string url, out int consumedLength)
            {
                url = null;
                consumedLength = 0;

                if (startIndex + HttpsPrefix.Length > text.Length)
                {
                    return false;
                }

                if (string.Compare(text, startIndex, HttpsPrefix, 0, HttpsPrefix.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }

                int endIndex = startIndex + HttpsPrefix.Length;
                while (endIndex < text.Length && !char.IsWhiteSpace(text[endIndex]) && text[endIndex] != '\r' && text[endIndex] != '\n')
                {
                    endIndex++;
                }

                int trimmedEndIndex = endIndex;
                while (trimmedEndIndex > startIndex + HttpsPrefix.Length && IsTrailingUrlPunctuation(text[trimmedEndIndex - 1]))
                {
                    trimmedEndIndex--;
                }

                if (trimmedEndIndex <= startIndex + HttpsPrefix.Length)
                {
                    return false;
                }

                consumedLength = trimmedEndIndex - startIndex;
                url = text.Substring(startIndex, consumedLength);
                return true;
            }

            private static bool IsTrailingUrlPunctuation(char character)
            {
                return character == '.'
                    || character == ','
                    || character == ';'
                    || character == ':'
                    || character == '!'
                    || character == '?'
                    || character == ')'
                    || character == ']'
                    || character == '}'
                    || character == '"';
            }

            private static void OnLinkRequestNavigate(object sender, RequestNavigateEventArgs e)
            {
                if (e?.Uri == null)
                {
                    return;
                }

                try
                {
                    Process.Start(e.Uri.AbsoluteUri);
                }
                catch
                {
                }

                e.Handled = true;
            }

            private static void FlushPlain(InlineCollection inlines, StringBuilder plain)
            {
                if (plain.Length == 0)
                {
                    return;
                }

                inlines.Add(new Run(plain.ToString()));
                plain.Clear();
            }
        }
    }
}
