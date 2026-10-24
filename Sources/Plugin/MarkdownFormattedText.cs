using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    public class MarkdownFormattedText : Span
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
            nameof(Key),
            typeof(string),
            typeof(MarkdownFormattedText),
            new PropertyMetadata(null, OnSourceChanged)
        );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MarkdownFormattedText),
            new PropertyMetadata(null, OnSourceChanged)
        );

        public string Key
        {
            get => (string)GetValue(KeyProperty);
            set => SetValue(KeyProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public MarkdownFormattedText()
        {
            Loaded += (_, __) => Refresh();
        }

        public static void AppendFormatted(InlineCollection inlines, string text, bool clearExisting = false)
        {
            InlineParser.AppendFormatted(inlines, text, clearExisting);
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MarkdownFormattedText)d).Refresh();
        }

        private void Refresh()
        {
            string value = ResolveSourceText();

            InlineParser.AppendFormatted(Inlines, value, true);
        }

        private string ResolveSourceText()
        {
            if (!string.IsNullOrWhiteSpace(Key))
            {
                return SLoc.GetValue(Key) ?? string.Empty;
            }

            return Text ?? string.Empty;
        }

        private static class InlineParser
        {
            private const string HttpsPrefix = "https://";
            private const double HeadingScale = 1.2;
            private const double HeadingMargin = 5.0;

            public static void AppendFormatted(InlineCollection inlines, string text, bool clearExisting = false)
            {
                if (inlines == null || string.IsNullOrEmpty(text))
                {
                    return;
                }

                if (clearExisting)
                {
                    inlines.Clear();
                }

                var plain = new StringBuilder();

                for (int index = 0; index < text.Length; index++)
                {
                    char current = text[index];

                    if (IsStartOfLine(text, index) && TryReadHeading(text, index, out var headingText, out var headingConsumedLength))
                    {
                        FlushPlain(inlines, plain);
                        AppendHeading(inlines, headingText);

                        index += headingConsumedLength - 1;
                        continue;
                    }

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

            private static bool IsStartOfLine(string text, int index)
            {
                return index == 0 || text[index - 1] == '\r' || text[index - 1] == '\n';
            }

            private static bool TryReadHeading(string text, int startIndex, out string headingText, out int consumedLength)
            {
                headingText = null;
                consumedLength = 0;

                if (text[startIndex] != '#' || startIndex + 1 >= text.Length || char.IsDigit(text[startIndex + 1]))
                {
                    return false;
                }

                int lineEndIndex = startIndex + 1;
                while (lineEndIndex < text.Length && text[lineEndIndex] != '\r' && text[lineEndIndex] != '\n')
                {
                    lineEndIndex++;
                }

                if (lineEndIndex <= startIndex + 1)
                {
                    return false;
                }

                int lineBreakLength = 0;
                if (lineEndIndex < text.Length)
                {
                    lineBreakLength = 1;
                    if (text[lineEndIndex] == '\r' && lineEndIndex + 1 < text.Length && text[lineEndIndex + 1] == '\n')
                    {
                        lineBreakLength = 2;
                    }
                }

                headingText = text.Substring(startIndex + 1, lineEndIndex - (startIndex + 1));
                consumedLength = (lineEndIndex - startIndex) + lineBreakLength;
                return true;
            }

            private static void AppendHeading(InlineCollection inlines, string headingText)
            {
                var heading = new Bold
                {
                    FontSize = new Run().FontSize * HeadingScale
                };

                AppendFormatted(heading.Inlines, headingText);
                inlines.Add(heading);
                inlines.Add(new LineBreak());
                inlines.Add(new InlineUIContainer(new Border { Height = HeadingMargin, Width = 0 }));
                inlines.Add(new LineBreak());
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
