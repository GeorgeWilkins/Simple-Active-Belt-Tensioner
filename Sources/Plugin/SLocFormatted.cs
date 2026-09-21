using System;
using System.Text;
using System.Windows;
using System.Windows.Documents;
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
