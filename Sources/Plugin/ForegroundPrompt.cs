using SimHub;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WoteverLocalization;

namespace User.ActiveBeltTensioner
{
    public class ForegroundPrompt : Window
    {
        private readonly TextBlock _messageTextBlock;
        private readonly ImageBrush _contextualImage;
        private readonly bool _requiresDecision;

        public string Message { get; set; } = string.Empty;
        public string Image { get; set; } = "Prompt.Default.png";
        public bool Result { get; private set; } = false;
        public string Value { get; private set; } = string.Empty;

        public ForegroundPrompt(string message, string image = null, bool requiresDecision = false, string inputDefault = null)
        {
            _requiresDecision = requiresDecision;

            Message = message;
            Image = string.IsNullOrWhiteSpace(image) ? "Prompt.Default.png" : image;

            bool requestsInput = (inputDefault != null);

            // Window Configuration & Styling
            Title = SLoc.GetValue("SABT_Plugin");
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.Height;
            Width = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Topmost = true;
            AllowsTransparency = true;
            Background = Brushes.Transparent;

            Border outerBorder = new Border
            {
                Background = (Brush)new BrushConverter().ConvertFrom("#202020"),
                Margin = new Thickness(10),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 15,
                    ShadowDepth = 5,
                    Opacity = 0.5
                }
            };

            // Define Layout
            Grid layoutGrid = new Grid { ClipToBounds = true };
            layoutGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layoutGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layoutGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250, GridUnitType.Pixel) });
            layoutGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Contextual Image
            _contextualImage = new ImageBrush
            {
                Stretch = Stretch.UniformToFill,
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center,
                ImageSource = LoadBitmap(Image)
            };

            Border imageBorder = new Border
            {
                Background = _contextualImage
            };
            Grid.SetRowSpan(imageBorder, 3);
            Grid.SetRow(imageBorder, 0);
            Grid.SetColumn(imageBorder, 0);

            layoutGrid.Children.Add(imageBorder);

            // SABT Logo
            Image brandLogoImage = new Image
            {
                Width = 60,
                Margin = new Thickness(15, 0, 0, -10),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Stretch = Stretch.Uniform,
                Source = LoadBitmap("SABT Logo.png"),
            };
            Grid.SetRow(brandLogoImage, 2);
            Grid.SetColumn(brandLogoImage, 0);

            layoutGrid.Children.Add(brandLogoImage);

            // Formatted Message Text
            _messageTextBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#FFFFFF"),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(15, 15, 15, 15)
            };
            Grid.SetRow(_messageTextBlock, 0);
            Grid.SetColumn(_messageTextBlock, 1);

            layoutGrid.Children.Add(_messageTextBlock);

            MarkdownFormattedText.AppendFormatted(_messageTextBlock.Inlines, Message ?? string.Empty, true);

            // Text Input Field
            TextBox inputTextBox = new TextBox
            {
                Text = inputDefault ?? string.Empty,
                Margin = new Thickness(15, -5, 15, 15),
                Visibility = requestsInput ? Visibility.Visible : Visibility.Collapsed
            };

            Loaded += (sender, e) =>
            {
                inputTextBox.Focus();
                inputTextBox.SelectAll();
            };

            Grid.SetRow(inputTextBox, 1);
            Grid.SetColumn(inputTextBox, 1);

            layoutGrid.Children.Add(inputTextBox);

            // Acknowledgment & Decision Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 15, 15)
            };

            Action<bool> closeWithResult = result =>
            {
                Result = result;
                Value = inputTextBox != null ? inputTextBox.Text : string.Empty;

                Close();
            };

            if (_requiresDecision)
            {
                Button yesButton = new Button { MinWidth = 60, Height = 30, Margin = new Thickness(0, 0, 0, 0), Content = "Yes" };
                yesButton.Click += (sender, e) => closeWithResult(true);
                buttonPanel.Children.Add(yesButton);

                Button noButton = new Button { MinWidth = 60, Height = 30, Margin = new Thickness(10, 0, 0, 0), Content = "No" };
                noButton.Click += (sender, e) => closeWithResult(false);
                buttonPanel.Children.Add(noButton);
            }
            else
            {
                Button okButton = new Button { MinWidth = 60, Height = 30, Content = "OK" };
                okButton.Click += (sender, e) => closeWithResult(true);
                buttonPanel.Children.Add(okButton);
            }

            Grid.SetRow(buttonPanel, 2);
            Grid.SetColumn(buttonPanel, 1);

            layoutGrid.Children.Add(buttonPanel);

            // Wrap Layout & Allow Dragging
            outerBorder.Child = layoutGrid;
            outerBorder.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => DragMove();

            Content = outerBorder;
        }

        public static bool ShowDialog(string message, string image = null, bool requiresDecision = false)
        {
            bool result = false;

            Action showDialogAction = () =>
            {
                ForegroundPrompt window = new ForegroundPrompt(
                    message: message,
                    image: image,
                    requiresDecision: requiresDecision
                );

                window.ShowDialog();

                result = window.Result;
            };

            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Dispatcher dispatcher = Application.Current.Dispatcher;

                if (dispatcher.CheckAccess())
                {
                    showDialogAction();
                }
                else
                {
                    dispatcher.Invoke(showDialogAction);
                }
            }
            else
            {
                showDialogAction();
            }

            return result;
        }

        public static string ShowDialog(string message, string image, string inputDefault = "")
        {
            bool result = false;
            string value = inputDefault;

            Action showDialogAction = () =>
            {
                ForegroundPrompt window = new ForegroundPrompt(
                    message: message,
                    image: image,
                    requiresDecision: false,
                    inputDefault: inputDefault
                );

                window.ShowDialog();

                result = window.Result;
                value = window.Value;
            };

            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Dispatcher dispatcher = Application.Current.Dispatcher;

                if (dispatcher.CheckAccess())
                {
                    showDialogAction();
                }
                else
                {
                    dispatcher.Invoke(showDialogAction);
                }
            }
            else
            {
                showDialogAction();
            }

            return result ? value : null;
        }

        private static ImageSource LoadBitmap(string imageFile)
        {
            string imagePath = imageFile.Replace('\\', '/').TrimStart('/');
            string assemblyName = typeof(ForegroundPrompt).Assembly.GetName().Name;
            string packUri = "pack://application:,,,/" + assemblyName + ";component/Graphics/" + Uri.EscapeUriString(imagePath);

            try
            {
                BitmapImage bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.UriSource = new Uri(packUri, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }
    }
}
