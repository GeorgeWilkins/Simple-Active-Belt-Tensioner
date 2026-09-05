using System;
using System.Windows;
using System.Windows.Controls;

namespace User.ActiveBeltTensioner
{
    public partial class SettingRangeSlider : UserControl
    {
        private bool _isSynchronizingValues;
        private bool _isSynchronizingMirroredValues;
        private bool _isSynchronizingMidpoint;
        private bool _isMidpointAuto = true;

        public SettingRangeSlider()
        {
            InitializeComponent();
            UpdateMidpoint();
            SetCurrentValue(LeftSliderValueProperty, MapLeftValueToSliderValue(LeftValue));
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
            new PropertyMetadata(1d));

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

        public static readonly DependencyProperty LeftSliderValueProperty = DependencyProperty.Register(
            nameof(LeftSliderValue),
            typeof(double),
            typeof(SettingRangeSlider),
            new FrameworkPropertyMetadata(
                0d,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnLeftSliderValueChanged,
                CoerceLeftSliderValue));

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

        public double LeftSliderValue
        {
            get { return (double)GetValue(LeftSliderValueProperty); }
            set { SetValue(LeftSliderValueProperty, value); }
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            control.CoerceValue(MidpointProperty);
            control.UpdateMidpoint();
            control.CoerceValue(LeftValueProperty);
            control.CoerceValue(RightValueProperty);
            control.CoerceValue(LeftSliderValueProperty);
            control.SetCurrentValue(LeftSliderValueProperty, control.MapLeftValueToSliderValue(control.LeftValue));

            if (control.ShouldMirror)
            {
                control.ApplyMirroredValuesFromRight();
            }
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
            if (control._isSynchronizingValues)
            {
                return;
            }

            control._isSynchronizingValues = true;
            control.SetCurrentValue(LeftSliderValueProperty, control.MapLeftValueToSliderValue((double)e.NewValue));
            control._isSynchronizingValues = false;

            if (control.ShouldMirror && !control._isSynchronizingMirroredValues)
            {
                control.ApplyMirroredValuesFromLeft();
            }
        }

        private static void OnRightValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            if (control.ShouldMirror && !control._isSynchronizingMirroredValues)
            {
                control.ApplyMirroredValuesFromRight();
            }
        }

        private static void OnLeftSliderValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            if (control._isSynchronizingValues)
            {
                return;
            }

            control._isSynchronizingValues = true;
            control.SetCurrentValue(LeftValueProperty, control.MapLeftSliderValueToValue((double)e.NewValue));
            control._isSynchronizingValues = false;

            if (control.ShouldMirror && !control._isSynchronizingMirroredValues)
            {
                control.ApplyMirroredValuesFromLeft();
            }
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
            return Clamp((double)baseValue, control.Minimum, control.GetEffectiveMidpoint());
        }

        private static object CoerceRightValue(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            return Clamp((double)baseValue, control.GetEffectiveMidpoint(), control.Maximum);
        }

        private static object CoerceLeftSliderValue(DependencyObject d, object baseValue)
        {
            var control = (SettingRangeSlider)d;
            return Clamp((double)baseValue, control.Minimum, control.GetEffectiveMidpoint());
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
            control.CoerceValue(LeftSliderValueProperty);
            control.SetCurrentValue(LeftSliderValueProperty, control.MapLeftValueToSliderValue(control.LeftValue));

            if (control.ShouldMirror)
            {
                control.ApplyMirroredValuesFromRight();
            }
        }

        private static void OnShouldMirrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SettingRangeSlider)d;
            if ((bool)e.NewValue)
            {
                control.ApplyMirroredValuesFromRight();
            }
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

        private double MapLeftValueToSliderValue(double leftValue)
        {
            return Minimum + GetEffectiveMidpoint() - leftValue;
        }

        private double MapLeftSliderValueToValue(double leftSliderValue)
        {
            return Minimum + GetEffectiveMidpoint() - leftSliderValue;
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
