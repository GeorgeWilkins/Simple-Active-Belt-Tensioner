using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace User.ActiveBeltTensioner
{
    public partial class SettingSlider : UserControl
    {
        public SettingSlider()
        {
            InitializeComponent();
            IsEnabledChanged += (s, e) =>
            {
                UpdateSliderOpacity();
                UpdateResetButtonVisibility();
            };
            UpdateDisplayValue();
        }

        private void UpdateSliderOpacity()
        {
            SliderControl.Opacity = IsEnabled ? 1.0 : 0.5;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            UpdateSliderOpacity();
            UpdateDisplayValue();
            UpdateResetButtonVisibility();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingSlider),
            new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
            nameof(Unit),
            typeof(string),
            typeof(SettingSlider),
            new PropertyMetadata(string.Empty));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(SettingSlider),
            new PropertyMetadata(0d));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(SettingSlider),
            new PropertyMetadata(100d));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(SettingSlider),
            new PropertyMetadata(1d, OnStepChanged));

        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(SettingSlider),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ShowResetProperty = DependencyProperty.Register(
            nameof(ShowReset),
            typeof(bool),
            typeof(SettingSlider),
            new PropertyMetadata(false, OnShowResetChanged));

        public bool ShowReset
        {
            get { return (bool)GetValue(ShowResetProperty); }
            set { SetValue(ShowResetProperty, value); }
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SettingSlider)d).UpdateDisplayValue();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SettingSlider)d).UpdateDisplayValue();
        }

        private static void OnShowResetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SettingSlider)d).UpdateResetButtonVisibility();
        }

        private void UpdateDisplayValue()
        {
            var valueTextBlock = FindName("ValueTextBlock") as TextBlock;
            if (valueTextBlock != null)
            {
                valueTextBlock.Text = FormatValueForStep(Value, Step);
            }
        }

        private void UpdateResetButtonVisibility()
        {
            var resetButton = FindName("ResetButton") as Button;
            if (resetButton != null)
            {
                resetButton.Visibility = (ShowReset && IsEnabled) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static string FormatValueForStep(double value, double step)
        {
            var decimals = GetDecimalPlaces(step);
            return value.ToString($"F{decimals}", CultureInfo.CurrentCulture);
        }

        private static int GetDecimalPlaces(double step)
        {
            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                return 0;
            }

            var text = step.ToString("0.#############################", CultureInfo.InvariantCulture);
            var decimalSeparatorIndex = text.IndexOf('.');
            return decimalSeparatorIndex < 0 ? 0 : text.Length - decimalSeparatorIndex - 1;
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            Value = Clamp(0d, Minimum, Maximum);
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (minimum > maximum)
            {
                var temporary = minimum;
                minimum = maximum;
                maximum = temporary;
            }

            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
