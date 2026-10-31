using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace User.ActiveBeltTensioner
{
    public partial class SettingRangeSlider : UserControl
    {
        private bool _isSynchronizingMirroredValues;
        private bool _isSynchronizingMidpoint;
        private bool _isMidpointAuto = true;

        public SettingRangeSlider()
        {
            InitializeComponent();
            UpdateMidpoint();
            IsEnabledChanged += (s, e) => UpdateSlidersOpacity();
            UpdateSlidersOpacity();
            UpdateDisplayValues();
        }

        private void UpdateSlidersOpacity()
        {
            RangeSliderControl.Opacity = IsEnabled ? 1.0 : 0.5;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            UpdateSlidersOpacity();
            UpdateDisplayValues();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(SettingRangeSlider),
            new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(
            nameof(Unit),
            typeof(string),
            typeof(SettingRangeSlider),
            new PropertyMetadata(string.Empty));

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(SettingRangeSlider),
            new PropertyMetadata(0d, OnRangeChanged));

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(SettingRangeSlider),
            new PropertyMetadata(100d, OnRangeChanged));

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty StepProperty = DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(SettingRangeSlider),
            new PropertyMetadata(1d, OnStepChanged));

        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        public static readonly DependencyProperty ShouldMirrorProperty = DependencyProperty.Register(
            nameof(ShouldMirror),
            typeof(bool),
            typeof(SettingRangeSlider),
            new PropertyMetadata(false, OnShouldMirrorChanged));

        public bool ShouldMirror
        {
            get { return (bool)GetValue(ShouldMirrorProperty); }
            set { SetValue(ShouldMirrorProperty, value); }
        }

        public static readonly DependencyProperty MidpointProperty = DependencyProperty.Register(
            nameof(Midpoint),
            typeof(double),
            typeof(SettingRangeSlider),
            new FrameworkPropertyMetadata(
                double.NaN,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnMidpointChanged,
                CoerceMidpoint));

        public double Midpoint
        {
            get { return (double)GetValue(MidpointProperty); }
            set { SetValue(MidpointProperty, value); }
        }

        public static readonly DependencyProperty LeftValueProperty = DependencyProperty.Register(
            nameof(LeftValue),
            typeof(double),
            typeof(SettingRangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLeftValueChanged,
                CoerceLeftValue));

        public static readonly DependencyProperty RightValueProperty = DependencyProperty.Register(
            nameof(RightValue),
            typeof(double),
            typeof(SettingRangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnRightValueChanged,
                CoerceRightValue));

        public double LeftValue
        {
            get { return (double)GetValue(LeftValueProperty); }
            set { SetValue(LeftValueProperty, value); }
        }

        public double RightValue
        {
            get { return (double)GetValue(RightValueProperty); }
            set { SetValue(RightValueProperty, value); }
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            control.CoerceValue(MidpointProperty);
            control.UpdateMidpoint();
            control.CoerceValue(LeftValueProperty);
            control.CoerceValue(RightValueProperty);

            if (control.ShouldMirror)
            {
                control.ApplyMirroredValuesFromRight();
            }

            control.UpdateDisplayValues();
        }

        private void UpdateMidpoint()
        {
            if (!_isMidpointAuto)
            {
                return;
            }

            _isSynchronizingMidpoint = true;
            SetCurrentValue(MidpointProperty, CalculateMidpoint(Minimum, Maximum));
            _isSynchronizingMidpoint = false;
        }

        private static void OnLeftValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            if (control.ShouldMirror && !control._isSynchronizingMirroredValues)
            {
                control.ApplyMirroredValuesFromLeft();
            }

            control.UpdateDisplayValues();
        }

        private static void OnRightValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            if (control.ShouldMirror && !control._isSynchronizingMirroredValues)
            {
                control.ApplyMirroredValuesFromRight();
            }

            control.UpdateDisplayValues();
        }

        private static object CoerceMidpoint(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            var midpoint = (double)baseValue;

            if (double.IsNaN(midpoint))
            {
                return double.NaN;
            }

            return Clamp(midpoint, control.Minimum, control.Maximum);
        }

        private static object CoerceLeftValue(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            var maximum = control.ShouldMirror ? control.GetEffectiveMidpoint() : control.RightValue;
            return Clamp((double)baseValue, control.Minimum, maximum);
        }

        private static object CoerceRightValue(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            var minimum = control.ShouldMirror ? control.GetEffectiveMidpoint() : control.LeftValue;
            return Clamp((double)baseValue, minimum, control.Maximum);
        }

        private static void OnMidpointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;

            if (!control._isSynchronizingMidpoint)
            {
                var midpoint = (double)e.NewValue;
                control._isMidpointAuto = double.IsNaN(midpoint);

                if (control._isMidpointAuto)
                {
                    control.UpdateMidpoint();
                    return;
                }
            }

            control.CoerceValue(LeftValueProperty);
            control.CoerceValue(RightValueProperty);

            if (control.ShouldMirror)
            {
                control.ApplyMirroredValuesFromRight();
            }

            control.UpdateDisplayValues();
        }

        private static void OnShouldMirrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            control.CoerceValue(LeftValueProperty);
            control.CoerceValue(RightValueProperty);

            if ((bool)e.NewValue)
            {
                control.ApplyMirroredValuesFromRight();
            }

            control.UpdateDisplayValues();
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SettingRangeSlider)d).UpdateDisplayValues();
        }

        private void UpdateDisplayValues()
        {
            var leftValueTextBlock = FindName("LeftValueTextBlock") as TextBlock;
            if (leftValueTextBlock != null)
            {
                leftValueTextBlock.Text = FormatValueForStep(LeftValue, Step);
            }

            var rightValueTextBlock = FindName("RightValueTextBlock") as TextBlock;
            if (rightValueTextBlock != null)
            {
                rightValueTextBlock.Text = FormatValueForStep(RightValue, Step);
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

        private void ApplyMirroredValuesFromLeft()
        {
            _isSynchronizingMirroredValues = true;
            SetCurrentValue(RightValueProperty, MapMirroredValue(LeftValue));
            _isSynchronizingMirroredValues = false;
        }

        private void ApplyMirroredValuesFromRight()
        {
            _isSynchronizingMirroredValues = true;
            SetCurrentValue(LeftValueProperty, MapMirroredValue(RightValue));
            _isSynchronizingMirroredValues = false;
        }

        private double MapMirroredValue(double value)
        {
            return (2d * GetEffectiveMidpoint()) - value;
        }

        private double GetEffectiveMidpoint()
        {
            return double.IsNaN(Midpoint) ? CalculateMidpoint(Minimum, Maximum) : Midpoint;
        }

        private static double CalculateMidpoint(double minimum, double maximum)
        {
            if (minimum > maximum)
            {
                var temporary = minimum;
                minimum = maximum;
                maximum = temporary;
            }

            return minimum + ((maximum - minimum) / 2d);
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
